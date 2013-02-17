module AttributePreprocessing

open System
open System.Data
open System.Collections.Generic
open TableUtilities
open NeuralTypes
open Environment
open Evaluator
open PreprocessingUtilities

// Posso referenziare l'attributo corrente con "A"
// Pesante!!!
let mathExpression (attributes:string list) expression (table:DataTable) =
    let exp = parseExpression expression
    let env = new Environment()

    // Prima controllo che gli identificatori inseriti corrispondano ad attributi nella tabella
    // Qui possibile speculazione...
    checkExpression table exp ["A"]

    fillAttributesSeries env table

    for attName in attributes do
        let attIndex = table.Columns.IndexOf(attName)
        let seriesValue = env.EnvSeries.[attName]
        env.EnvSeries.Remove(attName) |> ignore
        env.EnvSeries.Remove("ATT"+attIndex.ToString()) |> ignore
        env.EnvSeries.Add("A", seriesValue)
        for row in table.Select() do
            fillAttributesSingle env row
            if env.EnvSingle.ContainsKey(attName) then          // Il valore corrente potrebbe essere Missing e quindi non esserci nell'env
                let singleValue = env.EnvSingle.[attName]
                env.EnvSingle.Remove(attName) |> ignore
                env.EnvSingle.Remove("ATT"+attIndex.ToString()) |> ignore
                env.EnvSingle.Add("A", singleValue)
            row.[attName] <- try
                                box (evalExpression exp env)          
                             with
                             | exn -> box DBNull.Value
        env.EnvSeries.Remove("A") |> ignore
        env.EnvSeries.Add(attName, seriesValue)
        env.EnvSeries.Add("ATT"+attIndex.ToString(), seriesValue)

let addExpression (attName:string) expression (table:DataTable) =
    let exp = parseExpression expression
    let env = new Environment()

    // Prima controllo che gli identificatori inseriti corrispondano ad attributi nella tabella
    // Qui possibile speculazione...
    checkExpression table exp []

    fillAttributesSeries env table

    // Qui potrei andare parallelo/asincrono
    let col = new AttributeDataColumn(AttributeType.Numeric)
    col.DataType <- typeof<double>
    col.ColumnName <- attName
    table.Columns.Add(col)
    for row in table.Select() do
        fillAttributesSingle env row
         // L'istruzione sotto non serve perché alle righe del nuovo attributo non sono ancora stati assegnati dei valori, quindi non passa il check row.IsNull(colName) in fillAttributesSingle 
//        env.EnvSingle.Remove(attName) |> ignore             // Rimuovo il nuovo attributo che sto inserendo altrimenti si potrebbero scrivere delle espressioni "strane"
        row.[attName] <- try
                            box (evalExpression exp env)          
                         with
                         | exn -> box DBNull.Value          // Dopo il check fatto prima, se la valutazione fallisce significa che nell'espressione c'è un attributo che per la riga
                                                            // corrente ha m valore Missing, quindi il risultato dell'espressione non può che essere Missing

let normalize scale translation (table:DataTable) =
    let expression = sprintf "((A-min(A))/(max(A)-min(A))*%f)+%f" scale translation

    let attList = getAttributesByType AttributeType.Numeric table 
    mathExpression attList expression table


let standardize (table:DataTable) =
    let expression = "(A-mean(A))/(sd(A))"

    let attList = getAttributesByType AttributeType.Numeric table
    mathExpression attList expression table

let removeByName (attributes:string list) (table:DataTable) =
    attributes
    |> List.iter (fun name -> table.Columns.Remove(name) )

let removeByType (attributeType:AttributeType) (table:DataTable) = 
    getAttributesByType attributeType table
    |> List.iter(fun name -> table.Columns.Remove(name) )

let replaceMissingValues (table:DataTable) = 
    // Sostituisco i valori missing numerici con la media
    let means = getAttributesByType AttributeType.Numeric table
                |> List.map (fun name ->    let mean = Convert.ToDouble(table.Compute("Avg("+name+")",""))      
                                            (name, mean) )
    table.Select()
    |> Seq.iter (fun row -> means
                            |> List.iter (fun (colName, value) ->   if row.IsNull(colName) then 
                                                                        row.[colName] <- value ) )

    // Sostituisco i valori missing nominal con la moda (valore più frequente)
    let colNameAndMode = getAttributesByType (AttributeType.Nominal([])) table
                            |> List.map (fun name -> let nomList = match (table.Columns.[name] :?> AttributeDataColumn).AttributeType with
                                                                    | AttributeType.Nominal list -> list
                                                     let maxEl,_ = nomList
                                                                    |> List.map (fun nomEl ->   let occurencies = Convert.ToInt32(table.Compute("Count("+name+")", name+"='"+nomEl+"'"))
                                                                                                (nomEl, occurencies) )    
                                                                    |> List.maxBy (fun (_, occ) -> occ)   
                                                     (name, maxEl) )
    table.Select()
    |> Seq.iter (fun row -> colNameAndMode
                            |> List.iter (fun (colName, value) ->   if row.IsNull(colName) then 
                                                                        row.[colName] <- value ) )

let nominalToBinary (attributes:string list) (table:DataTable) = 
    let attNamesAndElements = getAttributesByType (AttributeType.Nominal([])) table
                                |> List.filter (fun name -> List.exists (fun attName -> attName = name) attributes )
                                |> List.map (fun name ->   let nomList = match (table.Columns.[name] :?> AttributeDataColumn).AttributeType with
                                                                                                            | AttributeType.Nominal list -> list 
                                                           nomList
                                                           |> List.iter (fun nomEl ->  let column = new AttributeDataColumn(AttributeType.Numeric)
                                                                                       column.ColumnName <- name+"="+nomEl 
                                                                                       column.DataType <- typeof<double>
                                                                                       table.Columns.Add(column) )
                                                           (name, nomList) )
    for row in table.Rows do
        for attName, elList in attNamesAndElements do
            for el in elList do
                let newName = attName+"="+el
                if row.[attName].ToString() = el then
                    row.[newName] <- 1.0
                else
                    row.[newName] <- 0.0

    attNamesAndElements
    |> List.iter (fun (attName,_) -> table.Columns.Remove(attName) )

let discretize (attributes:string list) (bins:int) (table:DataTable) = 
    if bins <= 0 then
        failwith "'bins' must be greater than 0"

    let minAndMax = getAttributesByType AttributeType.Numeric table
                        |> List.filter (fun name -> List.exists (fun attName -> attName = name) attributes )
                        |> List.map (fun name ->    let min = Convert.ToDouble(table.Compute("min("+name+")",""))
                                                    let max = Convert.ToDouble(table.Compute("max("+name+")",""))
                                                    (name, min, max) )

    for (name,min,max) in minAndMax do
        let step = (max-min)/double bins
        let mutable prec = min
        let nomElements = new Dictionary<string,DataRow[]>(HashIdentity.Structural)
        for i in 1..bins do
            if i = 1 then
                nomElements.Add("(-inf-"+min.ToString()+"]", table.Select(name+"<="+min.ToString(cultureInfo)))           // Primo estremo
            elif i = bins then
                nomElements.Add("("+max.ToString()+"-inf)", table.Select(name+">"+max.ToString(cultureInfo)))             // Ultimo estremo
            else
                let filter = String.Format(cultureInfo, "({0}>{1})and({0}<={2})", name, prec, prec+step)
                nomElements.Add("("+prec.ToString()+"-"+Convert.ToString((prec+step))+"]", table.Select(filter))
                prec <-(prec+step)
        
        let colIndex = table.Columns.[name].Ordinal
        table.Columns.Remove(name)

        let column = new AttributeDataColumn(AttributeType.Nominal(nomElements.Keys |> Seq.toList))
        column.ColumnName <- name
        column.DataType <- typeof<string>
        table.Columns.Add(column)
        column.SetOrdinal(colIndex)
        
        nomElements
        |> Seq.iter(fun el -> el.Value
                                |> Array.iter(fun row -> row.[name] <- el.Key) )

        

               




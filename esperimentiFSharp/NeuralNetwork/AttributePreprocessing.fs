module AttributePreprocessing

open System
open System.Data
open System.Collections.Generic
open TableUtilities
open NeuralTypes

let private cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-us")

// Individuo tutti gli attributi di un certo tipo
let private getAttributesByType (attType:AttributeType) (table:DataTable) =
    let colNames = new ResizeArray<string>()
    for col in table.Columns do
        if (col :?> AttributeDataColumn).AttributeType.GetType() = attType.GetType() then
            colNames.Add(col.ColumnName)
    List.ofSeq colNames


let mathExpression (attributesExpressions : (string*string) list) (table:DataTable) =
    attributesExpressions
    |> List.iteri (fun index (attName, exp) -> table.Columns.Add(attName+index.ToString(),typeof<double>, exp) |> ignore )

    for row in table.Rows do
        let mutable i = 0
        for colName,_ in attributesExpressions do
            row.[colName] <- row.[colName+i.ToString()]
            i <- i+1

    attributesExpressions
    |> List.iteri (fun index (attName, _) -> table.Columns.Remove(attName+index.ToString()) )

let addExpression (attName:string) expression (table:DataTable) =
    let col = new AttributeDataColumn(AttributeType.Numeric)
    col.DataType <- typeof<double>
    col.ColumnName <- attName
    table.Columns.Add(col)
    mathExpression [(attName, expression)] table

let normalize scale translation (table:DataTable) =
    let expression = sprintf "(({0}-{1})/({2}-{1})*%f)+%f" scale translation

    let expressions = getAttributesByType AttributeType.Numeric table
                        |> List.map (fun colName -> let min = sprintf "%.10f" (Convert.ToDouble(table.Compute("min("+colName+")","")))      // precomputation: abbasso il tempo da più di 4 minuti a molto meno di 1 sec
                                                    let max = sprintf "%.10f" (Convert.ToDouble(table.Compute("max("+colName+")","")))
                                                    (colName, String.Format(expression, colName, min, max)) )
    mathExpression expressions table


let standardize (table:DataTable) =
    let expression = "({0}-{1})/({2})"

    let expressions = getAttributesByType AttributeType.Numeric table
                        |> List.map ( fun colName ->    let mean = sprintf "%.10f" (Convert.ToDouble(table.Compute("Avg("+colName+")","")))      // precomputation: abbasso il tempo da più di 4 minuti a molto meno di 1 sec
                                                        let stdev = sprintf "%.10f" (Convert.ToDouble(table.Compute("StDev("+colName+")","")))
                                                        (colName, String.Format(expression, colName, mean, stdev)) )
    mathExpression expressions table

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

        

               




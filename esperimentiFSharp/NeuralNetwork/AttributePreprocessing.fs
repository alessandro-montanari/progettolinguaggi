module AttributePreprocessing

open System
open System.Data
open System.Collections.Generic
open TableUtilities
open NeuralTypes

let private cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-us")

// Controllare che gli attributi a cui si applica siano numerici
let mathExpression (attsExpressions : (string*string) list) (dt:DataTable) =
    attsExpressions
    |> List.iteri (fun index (attName, exp) -> dt.Columns.Add(attName+index.ToString(),typeof<double>, exp) |> ignore )

    for row in dt.Rows do
        let mutable i = 0
        for colName,_ in attsExpressions do
            row.[colName] <- row.[colName+i.ToString()]
            i <- i+1

    attsExpressions
    |> List.iteri (fun index (attName, _) -> dt.Columns.Remove(attName+index.ToString()) )

let addExpression (attName:string) expression (dt:DataTable) =
    let col = new AttributeDataColumn(AttributeType.Numeric)
    col.DataType <- typeof<double>
    col.ColumnName <- attName
    dt.Columns.Add(col)
    mathExpression [(attName, expression)] dt

// Individuo tutti gli attributi di un certo tipo
let private getAttributesByType (attType:AttributeType) (dt:DataTable) =
    let colNames = new ResizeArray<string>()
    for col in dt.Columns do
        if (col :?> AttributeDataColumn).AttributeType.GetType() = attType.GetType() then
            colNames.Add(col.ColumnName)
    List.ofSeq colNames

// Controllare che scale e translation siano dei numeri validi
let normalize scale translation (dt:DataTable) =
    let expression = sprintf "(({0}-{1})/({2}-{1})*%f)+%f" scale translation

    let expressions = getAttributesByType AttributeType.Numeric dt
                        |> List.map (fun colName -> let min = sprintf "%.10f" (Convert.ToDouble(dt.Compute("min("+colName+")","")))      // precomputation: abbasso il tempo da più di 4 minuti a molto meno di 1 sec
                                                    let max = sprintf "%.10f" (Convert.ToDouble(dt.Compute("max("+colName+")","")))
                                                    (colName, String.Format(expression, colName, min, max)) )
    mathExpression expressions dt


let standardize (dt:DataTable) =
    let expression = "({0}-{1})/({2})"

    let expressions = getAttributesByType AttributeType.Numeric dt
                        |> List.map ( fun colName ->    let mean = sprintf "%.10f" (Convert.ToDouble(dt.Compute("Avg("+colName+")","")))      // precomputation: abbasso il tempo da più di 4 minuti a molto meno di 1 sec
                                                        let stdev = sprintf "%.10f" (Convert.ToDouble(dt.Compute("StDev("+colName+")","")))
                                                        (colName, String.Format(expression, colName, mean, stdev)) )
    mathExpression expressions dt

let removeByName (atts:string list) (dt:DataTable) =
    atts
    |> List.iter (fun name -> dt.Columns.Remove(name) )

let removeByType (attType:AttributeType) (dt:DataTable) = 
    getAttributesByType attType dt
    |> List.iter(fun name -> dt.Columns.Remove(name) )

let replaceMissingValues (dt:DataTable) = 
    // Prima gli attributi numerici
    let means = getAttributesByType AttributeType.Numeric dt
                |> List.map (fun name ->    let mean = Convert.ToDouble(dt.Compute("Avg("+name+")",""))
                                            (name, mean) )
    dt.Select()
    |> Seq.iter (fun row -> means
                            |> List.iter (fun (colName, value) ->   if row.IsNull(colName) then 
                                                                        row.[colName] <- value ) )

    // Poi quelli nominali
    let colNameAndMode = getAttributesByType (AttributeType.Nominal([])) dt
                            |> List.map (fun name -> let nomList = match (dt.Columns.[name] :?> AttributeDataColumn).AttributeType with
                                                                    | AttributeType.Nominal list -> list
                                                     let maxEl,_ = nomList
                                                                    |> List.map (fun nomEl ->   let occurencies = Convert.ToInt32(dt.Compute("Count("+name+")", name+"='"+nomEl+"'"))
                                                                                                (nomEl, occurencies) )    
                                                                    |> List.maxBy (fun (_, occ) -> occ)   
                                                     (name, maxEl) )
    dt.Select()
    |> Seq.iter (fun row -> colNameAndMode
                            |> List.iter (fun (colName, value) ->   if row.IsNull(colName) then 
                                                                        row.[colName] <- value ) )

let nominalToBinary (atts:string list) (dt:DataTable) = 
    let attNamesAndElements = getAttributesByType (AttributeType.Nominal([])) dt
                                |> List.filter (fun name -> List.exists (fun attName -> attName = name) atts )
                                |> List.map (fun name ->   let nomList = match (dt.Columns.[name] :?> AttributeDataColumn).AttributeType with
                                                                                                            | AttributeType.Nominal list -> list 
                                                           nomList
                                                           |> List.iter (fun nomEl ->  let column = new AttributeDataColumn(AttributeType.Numeric)
                                                                                       column.ColumnName <- name+"="+nomEl 
                                                                                       column.DataType <- typeof<double>
                                                                                       dt.Columns.Add(column) )
                                                           (name, nomList) )
    for row in dt.Rows do
        for attName, elList in attNamesAndElements do
            for el in elList do
                let newName = attName+"="+el
                if row.[attName].ToString() = el then
                    row.[newName] <- 1.0
                else
                    row.[newName] <- 0.0

    attNamesAndElements
    |> List.iter (fun (attName,_) -> dt.Columns.Remove(attName) )

let discretize (atts:string list) (bins:int) (dt:DataTable) = 
    let minAndMax = getAttributesByType AttributeType.Numeric dt
                        |> List.filter (fun name -> List.exists (fun attName -> attName = name) atts )
                        |> List.map (fun name ->    let min = Convert.ToDouble(dt.Compute("min("+name+")",""))
                                                    let max = Convert.ToDouble(dt.Compute("max("+name+")",""))
                                                    (name, min, max) )

    for (name,min,max) in minAndMax do
        let step = (max-min)/double bins
        let mutable prec = min
        let nomElements = new Dictionary<string,DataRow[]>(HashIdentity.Structural)
        for i in 1..bins do
            if i = 1 then
                nomElements.Add("(-inf-"+min.ToString()+"]", dt.Select(name+"<="+min.ToString(cultureInfo)))           // Primo estremo
            elif i = bins then
                nomElements.Add("("+max.ToString()+"-inf)", dt.Select(name+">"+max.ToString(cultureInfo)))             // Ultimo estremo
            else
                let filter = String.Format(cultureInfo, "({0}>{1})and({0}<={2})", name, prec, prec+step)
                nomElements.Add("("+prec.ToString()+"-"+Convert.ToString((prec+step))+"]", dt.Select(filter))
                prec <-(prec+step)
        
        let colIndex = dt.Columns.[name].Ordinal
        dt.Columns.Remove(name)

        let column = new AttributeDataColumn(AttributeType.Nominal(nomElements.Keys |> Seq.toList))
        column.ColumnName <- name
        column.DataType <- typeof<string>
        dt.Columns.Add(column)
        column.SetOrdinal(colIndex)
        
        nomElements
        |> Seq.iter(fun el -> el.Value
                                |> Array.iter(fun row -> row.[name] <- el.Key) )

        

               




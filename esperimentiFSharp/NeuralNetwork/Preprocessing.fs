module Preprocessing

open System
open System.Data
open TableUtilities
open NeuralTypes

let private cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-us")

// Per il preprocessing ci sarà "qualcosa" che prende i filtri e man mano li applica e alla fine sputa fuori una
// nuova DataTable (quella originale si perde, per efficienza)

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

// Controllare che scale e translation siano dei numeri validi
let normalize scale translation (dt:DataTable) =
    let expression = sprintf "(({0}-{1})/({2}-{1})*%f)+%f" scale translation
    
    // Individuo tutti gli attributi numerici
    let attsExpressions = new ResizeArray<string*string>()
    for col in dt.Columns do
        if (col :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then
            let min = sprintf "%.10f" (Convert.ToDouble(dt.Compute("min("+col.ColumnName+")","")))      // precomputation: abbasso il tempo da più di 4 minuti a molto meno di 1 sec
            let max = sprintf "%.10f" (Convert.ToDouble(dt.Compute("max("+col.ColumnName+")","")))
            attsExpressions.Add(col.ColumnName, String.Format(expression, col.ColumnName, min, max))

    mathExpression (List.ofArray (attsExpressions.ToArray())) dt

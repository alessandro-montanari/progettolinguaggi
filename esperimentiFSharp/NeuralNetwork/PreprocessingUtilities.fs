module PreprocessingUtilities

open System
open System.Data
open System.Collections.Generic
open TableUtilities
open NeuralTypes
open Environment
open Evaluator

let internal cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-us")

// Carica nell'Environment gli attributi come serie di valori
let internal fillAttributesSeries (env:Environment) (table:DataTable) = 
    env.EnvSeries.Clear()
    let mutable index = 0
    for col in table.Columns do
        let colName = col.ColumnName
        if (table.Columns.[colName] :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then         //TODO devo lasciar passare anche gli attributi stringa
            let list:double list = query {  for row in table.Select() do
                                            where ( not (row.IsNull(colName)) )                                 // Salto i valori missing
                                            select row.[colName]  } |> Seq.map unbox |> Seq.toList
            env.EnvSeries.Add(col.ColumnName, list)
            env.EnvSeries.Add("ATT"+index.ToString(), list)
            index <- index+1

// Carica nell'Environment gli attributi come valori singoli (una riga)
let internal fillAttributesSingle (env:Environment) (row:DataRow) = 
    env.EnvSingle.Clear()
    let table = row.Table
    let mutable index = 0
    for col in table.Columns do
        let colName = col.ColumnName
        if not(row.IsNull(colName)) && (table.Columns.[colName] :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then     // Se il valore dell'att è missing non lo metto nell'env
            env.EnvSingle.Add(colName, unbox row.[colName])
            env.EnvSingle.Add("ATT"+index.ToString(), unbox row.[colName])
            index <- index+1

// idList è la lista di identificatori addizionali che si vuole controllare se sono presenti o meno nell'espressione
let internal checkExpression (table:DataTable) exp (idList:string list) =
    let env = new Environment()
    let colNameList = [for col in table.Columns do yield col.ColumnName]
    let indexList = [for index in 0..table.Columns.Count-1 do yield "ATT"+index.ToString()]
    let list = colNameList @ indexList @ idList
    list
    |> List.iter(fun el -> env.EnvSingle.Add(el, 0.0))    
    checkExpression exp env

// Individuo tutti gli attributi di un certo tipo
let internal getAttributesByType (attType:AttributeType) (table:DataTable) =
    let colNames = new ResizeArray<string>()
    for col in table.Columns do
        if (col :?> AttributeDataColumn).AttributeType.GetType() = attType.GetType() then
            colNames.Add(col.ColumnName)
    List.ofSeq colNames


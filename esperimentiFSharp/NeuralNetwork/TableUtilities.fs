module TableUtilities

open System.Data
open NeuralTypes
open System.Collections.Generic
open System

/// Custom DataColumn to hold the AttributeType of the attribute represented by the column
type AttributeDataColumn =
    inherit DataColumn

    val mutable AttributeType : AttributeType
    new () = { inherit DataColumn(); AttributeType = AttributeType.String }
    new (attType) = { inherit DataColumn(); AttributeType = attType }       

/// Transforms the value storend in the specified DataRow, in the column specfied by the index, into an istance of AttributeValue
let toAttributeValue (row:DataRow) (index:int) =
    let attType = (row.Table.Columns.[index] :?> AttributeDataColumn).AttributeType
    if row.IsNull(index) then
        Missing
    else
        let value =  row.[index]
        match attType with
        | AttributeType.Numeric -> AttributeValue.Numeric(Convert.ToDouble(value))
        | AttributeType.String -> AttributeValue.String(value.ToString())
        | AttributeType.Nominal list -> let nomVal = value.ToString()
                                        let index = (List.findIndex (fun el -> el = nomVal) list) + 1
                                        AttributeValue.Nominal(nomVal, index) 

let buildTableFromArff filePath =
    let table = { new DataTable() with override d.Clone() =                                 // Object Expression - Sono costretto a sovrascrivere il Clone() per copiare anche l'AttributeType
                                            let tableClone = base.Clone()
                                            for col in d.Columns do
                                                if col.GetType() = typeof<AttributeDataColumn> then
                                                    let typ = (col :?> AttributeDataColumn).AttributeType
                                                    let newCol = tableClone.Columns.[col.ColumnName] :?> AttributeDataColumn
                                                    newCol.AttributeType <- typ   
                                            tableClone }
    let dataSet = ArffLanguageUtilities.parseFile filePath
    table.TableName <- dataSet.Relation
    dataSet.Attributes    
        |> List.iter (function | Attribute(name, typo) ->   let colType = match typo with
                                                                          | AttributeType.Numeric _ -> typeof<double>
                                                                          | AttributeType.Nominal _ | AttributeType.String -> typeof<string>
                                                            let col = new AttributeDataColumn(typo)
                                                            col.DataType <- colType
                                                            col.ColumnName <- name
                                                            table.Columns.Add(col) )                                        
    dataSet.Data
        |> Seq.iter (function | Instance(valList) ->    let row = table.NewRow()
                                                        let mutable i = 0
                                                        for value in valList do
                                                            match value with 
                                                            | Numeric v ->  row.[i] <- v
                                                            | String s | Nominal (s,_) ->  row.[i] <- s
                                                            | Missing -> row.[i] <- System.DBNull.Value
                                                            i <- i+1
                                                        table.Rows.Add(row) )
    table         
    
    
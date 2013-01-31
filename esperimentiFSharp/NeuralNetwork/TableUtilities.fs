module TableUtilities

open System.Data
open NeuralTypes
open System.Collections.Generic

type AttributeDataColumn =
    inherit DataColumn

    val mutable AttributeType : AttributeType
    new () = { inherit DataColumn(); AttributeType = AttributeType.String }
    new (attType) = { inherit DataColumn(); AttributeType = attType }

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
        |> List.iter (function | Attribute(name, typo) ->   let colType = typeof<AttributeValue>
                                                            let col = new AttributeDataColumn(typo)
                                                            col.DataType <- colType
                                                            col.ColumnName <- name
                                                            table.Columns.Add(col) |> ignore)                                        
    dataSet.Data
        |> Seq.iter (function | Instance(valList) ->    let row = table.NewRow()
                                                        let mutable i = 0
                                                        for value in valList do
                                                            row.[i] <- value
                                                            i <- i+1
                                                        table.Rows.Add(row) )
    table         
    
    
module TableUtilities

open System.Data
open NeuralTypes
open System.Collections.Generic

type AttributeDataColumn(attType : AttributeType) =
    inherit DataColumn()

    member c.AttributeType = attType
                             

let buildTableFromArff filePath =
    let table = new DataTable()
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
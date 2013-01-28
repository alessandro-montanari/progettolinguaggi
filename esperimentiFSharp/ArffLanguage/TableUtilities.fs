module TableUtilities

//TODO potrebbe essere nel modulo di Arff

//TODO USARE INPUTVALUE CHE C'è IN NEURAL.FS

//open System.Data
//open ArffTypes
//open Neural
//open System.Collections.Generic
//
//let buildTableFromArff filePath =
//    let nomDict = new Dictionary<string, string list>(HashIdentity.Structural)
//    let table = new DataTable()
//    let arffFile = ArffLanguageUtilities.parseFile filePath
//    table.TableName <- arffFile.Relation
//    arffFile.Attributes    
//        |> List.map (function | ArffAttribute(name, typo) -> 
//                                                        let colType = match typo with
//                                                                            | ArffType.String | ArffType.Numeric -> typeof<InputValue>
//                                                                            | ArffType.Nominal(nomList) ->  nomDict.Add(name, nomList)
//                                                                                                            typeof<InputValue>
//                                                        table.Columns.Add(name, colType)) |> ignore
//                                                        
//    arffFile.Data
//        |> Seq.iter (function | ArffInstance(valList) -> 
//                                                        let row = table.NewRow()
//                                                        let mutable i = 0
//                                                        for value in valList do
//                                                            match value with
//                                                                | ArffValue.String(v) ->    let value = match nomDict.TryGetValue(v) with       // NO!!
//                                                                                                        | (result, _) when result -> InputValue.Nominal(v,0)
//                                                                                                        | _ -> InputValue.String(v) 
//                                                                                            row.[i] <- value
//                                                                | ArffValue.Numeric(v) -> row.[i] <- InputValue.Numeric(v)
//                                                                | ArffValue.Missing -> row.[i] <- InputValue.Missing
//                                                            i <- i+1
//                                                        table.Rows.Add(row) )
//    table    
//
//
//
//type MyDataColumn(myType : ArffType) =
//    inherit DataColumn()
//
//    member c.MyType = myType
//                                       
//    
//    
//
//

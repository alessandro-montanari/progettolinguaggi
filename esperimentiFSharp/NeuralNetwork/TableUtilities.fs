module TableUtilities

//TODO potrebbe essere nel modulo di Arff

//TODO USARE INPUTVALUE CHE C'è IN NEURAL.FS

open System.Data
open ArffTypes



let buildTableFromArff filePath =
    let table = new DataTable()
    let arffFile = ArffLanguageUtilities.parseFile filePath
    table.TableName <- arffFile.Relation
    arffFile.Attributes    
        |> List.map (function | ArffAttribute(name, typo) -> 
                                                        let colType = match typo with
                                                                            | ArffType.String | ArffType.Nominal(_) -> typeof<string>
                                                                            | ArffType.Numeric -> typeof<double> 
                                                        table.Columns.Add(name, colType) ) |> ignore
    arffFile.Data
        |> Seq.iter (function | ArffInstance(valList) -> 
                                                        let row = table.NewRow()
                                                        let mutable i = 0
                                                        for value in valList do
                                                            match value with
                                                                | ArffValue.String(v) -> row.[i] <- v
                                                                | ArffValue.Numeric(v) -> row.[i] <- v
                                                                | ArffValue.Missing -> row.[i] <- System.DBNull.Value 
                                                            i <- i+1
                                                        table.Rows.Add(row) )
    table    
                                       
    
    



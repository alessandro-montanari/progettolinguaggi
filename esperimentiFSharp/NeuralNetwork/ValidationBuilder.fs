module ValidationBuilder

open System
open System.Data
open System.Collections.Generic
open Parameter

type BasicValidationBuilder() =
    
    let createTestFromFile path =                                           // funzione privata
        TableUtilities.buildTableFromArff path

    let createTestFromSplit (trainingTable : DataTable) perc =              // funzione privata
        let random = new Random()
        let table = trainingTable.Clone()
        let maxRows = trainingTable.Rows.Count
        let numRows = Convert.ToInt32(float trainingTable.Rows.Count * (float perc/100.0))
        for i in 0 .. numRows do
            let index = random.Next(0, maxRows)
            table.Rows.Add(trainingTable.Rows.[index].ItemArray) |> ignore
        table   

    let initParameterTypes() =                                               // funzione privata
        let dic = new Dictionary<string, Type>(HashIdentity.Structural)
        dic.Add("TEST_SET", String("").GetType())          // Per indicare che "TEST_SET" può accettare solo stringhe
        dic.Add("PERCENTAGE_SPLIT", Number(0.0).GetType())   
        dic

    let _paramStore = new ParameterStore(initParameterTypes())

    member this.ParameterStore = _paramStore

    member this.BuildTestTable(trainingTable : DataTable) =
        if Seq.isEmpty this.ParameterStore.ParameterValues then              // Se non sono stati settati parametri utilizzo la training table
            trainingTable
        else
            if Seq.length this.ParameterStore.ParameterValues = 2 then
                failwith "It's not possible to have more than one parameter setted in BasicValidationBuilder"
            else
                match Seq.exactlyOne this.ParameterStore.ParameterValues with        // Sono sicuro che ci sia un solo elemento
                    | String(path) -> createTestFromFile path
                    | Number(perc) -> createTestFromSplit trainingTable perc
                    | _ -> failwith "never here!!"

    



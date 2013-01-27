module BuilderModule

open System
open System.Data
open System.Collections.Generic

// Idea:
// - il framework è indipendente dal linguaggio
// - i tipi dei parametri che possono essere settati nel framework sono ovviamente simili a quelli del linguaggio ma sono molto meno dettagliati di quelli dell'AST
// - per le espressioni, non si setta l'espressione in se (stringa) ma il suo risultato, quindi double o boolean. In pratica l'interprete del linguaggio quando incontra un'espression
// la valuta e setta il valore nel builder

type ParameterValue =
    | AttributeList of string list
    | NumberList of double list
    | InstanceList of int list
    | String of string
    | Number of double
    | Logic of bool

type ParameterStore(typeDic : Dictionary<string, Type>) as this =

    let _paramsDict = new Dictionary<string, ParameterValue>(HashIdentity.Structural)
    let _paramsTypeDict : Dictionary<string, Type> = typeDic

    member this.ParameterNames = _paramsTypeDict.Keys |> Seq.readonly
    member this.ParameterValues = _paramsDict.Values |> Seq.readonly
    member this.Parameters = _paramsDict |> Seq.readonly

    member this.SetValue(paramName, newValue) =
        let attType = _paramsTypeDict.[paramName]
        if newValue.GetType() <> attType then
            invalidArg "newValue" "Invalid argument type"
        else
            _paramsDict.[paramName] <- newValue        

    member this.GetValue(paramName) =
        _paramsDict.[paramName]

 
type BasicValidationBuilder() as this =
    
    let createTestFromFile path =                                           // funzione privata
        TableUtilities.buildTableFromArff path

    let createTestFromSplit (trainingTable : DataTable) perc =              // funzione privata
        let random = new Random()
        let table = new DataTable()    
        let maxRows = trainingTable.Rows.Count
        let numRows = Convert.ToInt32(float trainingTable.Rows.Count * (float perc/100.0))
        for i in 0 .. numRows do
            let index = random.Next(0, maxRows)
            table.Rows.Add(trainingTable.Rows.[index])
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

    

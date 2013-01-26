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

[<AbstractClass>]
type ParameterStore() as this =

    let _paramsDict = new Dictionary<string, ParameterValue>(HashIdentity.Structural)
    let _paramsTypeDict = new Dictionary<string, Type>(HashIdentity.Structural)

    do this.InitParameterTypes(_paramsTypeDict)

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

    abstract member InitParameterTypes : Dictionary<string, Type> -> unit                         // Viene invocato automaticamente dal costruttore e serve ad inizializzare il dizionario       
    
 
type BasicValidationBuilder() =
    inherit ParameterStore()            // Può usarlo invece che ereditare
    
    let createTestFromFile path =
        TableUtilities.buildTableFromArff path

    let createTestFromSplit (trainingTable : DataTable) perc =
        let random = new Random()
        let table = new DataTable()    
        let maxRows = trainingTable.Rows.Count
        let numRows = Convert.ToInt32(float trainingTable.Rows.Count * (float perc/100.0))
        for i in 0 .. numRows do
            let index = random.Next(0, maxRows)
            table.rows
        table   

    override vb.InitParameterTypes(dict) =
        dict.Add("TEST_SET", String("").GetType())          // Per indicare che "TEST_SET" può accettare solo stringhe
        dict.Add("PERCENTAGE_SPLIT", Number(0.0).GetType())   

    member vb.BuildTestTable(trainingTable : DataTable) =
        if Seq.isEmpty vb.ParameterValues then              // Se non sono stati settati parametri utilizzo la training table
            trainingTable
        else
            if Seq.length vb.ParameterValues = 2 then
                failwith "It's not possible to have more than one parameter setted in BasicValidationBuilder"
            else
                match Seq.exactlyOne vb.ParameterValues with        // Sono sicuro che ci sia un solo elemento
                    | String(path) -> createTestFromFile path
                    | Number(perc) -> createTestFromSplit trainingTable perc
                    | _ -> failwith "never here!!"

    

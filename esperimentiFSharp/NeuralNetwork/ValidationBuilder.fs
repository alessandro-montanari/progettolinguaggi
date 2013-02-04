module ValidationBuilder

open System
open System.Data
open System.Collections.Generic
open Parameter
open Builder

let private globalRules = new Dictionary<string, (string->obj->unit)>(HashIdentity.Structural)
globalRules.Add("TEST_SET", (fun name input -> if input.GetType() <> typeof<string> then
                                                        invalidArg name "Wrong type, expected 'string'"  ))

globalRules.Add("PERCENTAGE_SPLIT", (fun name input -> if input.GetType() <> typeof<double> then
                                                            invalidArg name "Wrong type, expected 'double'"
                                                       let value = unbox input
                                                       if value <= 0.0 && value > 100.0 then
                                                            invalidArg name "Must be grater than 0.0 and less than or equal to 100.0" ))
globalRules.Add("TRAINING_TABLE", (fun name input -> if not (typeof<DataTable>.IsInstanceOfType(input)) then
                                                        invalidArg name "Wrong type, expected 'DataTable'" ))

let private aspectsRules = new Dictionary<string, Dictionary<string,(string->obj->unit)>>(HashIdentity.Structural)

type BasicValidationBuilder() =
    inherit Builder<DataTable>(globalRules, aspectsRules)

    let check (builder:Builder<'T>) =
        let numOfAspects = builder.Aspects |> Seq.length
        let numOfGlobParams = builder.GlobalParameters.ParameterValues |> Seq.length
        let numOfTestSet = builder.GlobalParameters.GetValues("TEST_SET") |> Seq.length
        let numOfSplit = builder.GlobalParameters.GetValues("PERCENTAGE_SPLIT") |> Seq.length
        let numOfTable = builder.GlobalParameters.GetValues("TRAINING_TABLE") |> Seq.length
        if numOfAspects > 0 then
            failwith "It is not possible to set aspects in the BackPropagationBuilder"
        if numOfGlobParams > 2 then                                                                 // Massimo due parametri settati
            failwith "Only one global parameter must be set in BasicValidationBuilder, TEST_SET or PERCENTAGE_SPLIT"
        if numOfTestSet > 1 then
            failwith "Only one TEST_SET parameter can be setted in BasicValidationBuilder"
        if numOfSplit > 1 then
            failwith "Only one PERCENTAGE_SPLIT parameter can be setted in BasicValidationBuilder"
        if numOfTable <> 1 then
            failwith "Exactly one TRAINING_TABLE parameter must be setted in BasicValidationBuilder"
    
    let createTestFromFile path =                                           // funzione privata
        TableUtilities.buildTableFromArff path

    let createTestFromSplit (trainingTable : DataTable) (perc:double) =              // funzione privata
        let random = new Random()
        let table = trainingTable.Clone()
        let maxRows = trainingTable.Rows.Count
        let numRows = Convert.ToInt32(float trainingTable.Rows.Count * (perc/100.0))
        for i in 0 .. numRows do
            let index = random.Next(0, maxRows)
            table.Rows.Add(trainingTable.Rows.[index].ItemArray) |> ignore
        table   

    override this.Build() =
        check this
        let trainingTable = this.GlobalParameters.GetValues("TRAINING_TABLE") |> Seq.exactlyOne |> unbox
        let numOfGlobParams = this.GlobalParameters.ParameterValues |> Seq.length
        if numOfGlobParams = 1 then                                                     // Se non sono stati settati parametri utilizzo la training table
            trainingTable
        else
            match this.GlobalParameters.ParameterValues |> Seq.tryFind (fun (name,_) -> name = "TEST_SET") with
            | Some((_, objList)) -> createTestFromFile (objList |> Seq.exactlyOne |> unbox)
            | None ->  match this.GlobalParameters.ParameterValues |> Seq.tryFind (fun (name,_) -> name = "PERCENTAGE_SPLIT") with
                        | Some((_, objList)) -> createTestFromSplit trainingTable (objList |> Seq.exactlyOne |> unbox)
                        | None -> failwith "never here!!"

    override this.Name = "BasicValidationBuilder"




    



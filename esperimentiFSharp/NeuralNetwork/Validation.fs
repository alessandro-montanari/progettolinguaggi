module Validation

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

let private aspectsRules = new Dictionary<string, Dictionary<string,(string->obj->unit)>>(HashIdentity.Structural)

// potrei anche non usarlo
type BasicValidationBuilder() =
    inherit Builder<DataTable>(globalRules, aspectsRules)

    let check (builder:Builder<'T>) =
        let numOfAspects = builder.Aspects |> Seq.length
        let numOfLocParams = builder.LocalParameters.ParameterValues |> Seq.length
        let numOfTestSet = builder.LocalParameters.GetValues("TEST_SET") |> Seq.length
        let numOfSplit = builder.LocalParameters.GetValues("PERCENTAGE_SPLIT") |> Seq.length
        if numOfAspects > 0 then
            failwith "It is not possible to set aspects in the BackPropagationBuilder"
        if numOfLocParams > 2 then                                                                 // Massimo due parametri settati
            failwith "Only one global parameter must be set in BasicValidationBuilder, TEST_SET or PERCENTAGE_SPLIT"
        if numOfTestSet > 1 then
            failwith "Only one TEST_SET parameter can be setted in BasicValidationBuilder"
        if numOfSplit > 1 then
            failwith "Only one PERCENTAGE_SPLIT parameter can be setted in BasicValidationBuilder"
    
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

    override this.Build(gobalParameters:ParameterStore) =
        check this
        let trainingTable = gobalParameters.GetValues("TRAINING_TABLE") |> Seq.exactlyOne |> unbox          // Non controllo se c'è o meno il parametro, do per scontato che ci sia
        let numOfGlobParams = this.LocalParameters.ParameterValues |> Seq.length
        if numOfGlobParams = 1 then                                                     // Se non sono stati settati parametri utilizzo la training table
            trainingTable
        else
            match this.LocalParameters.ParameterValues |> Seq.tryFind (fun (name,_) -> name = "TEST_SET") with
            | Some((_, objList)) -> createTestFromFile (objList |> Seq.exactlyOne |> unbox)
            | None ->  match this.LocalParameters.ParameterValues |> Seq.tryFind (fun (name,_) -> name = "PERCENTAGE_SPLIT") with
                        | Some((_, objList)) -> createTestFromSplit trainingTable (objList |> Seq.exactlyOne |> unbox)
                        | None -> failwith "never here!!"

    override this.Name = "BasicValidationBuilder"

    override this.GetVisualizer(param) =
        null




    



module NetworkIterpreter

open System
open AST
open Neural
open Parameter
open System.Collections.Generic
open Preprocessing
open Builder
open System.Data
open TableUtilities
open Validation

let evalInstanceListElement nOfInstances = function
    | InstIndex idx -> if idx >= 0 && idx <= nOfInstances-1 then
                            [idx]
                       else
                             failwithf "An instance with index '%d' is not defined" idx
    | InstSequence(idx1, idx2) -> let indexList = if idx1 <= idx2 then
                                                        [idx1..idx2]
                                                  else
                                                        [idx1..(-1)..idx2]
                                  if List.forall(fun el ->  el >= 0 && el <= nOfInstances-1) indexList then
                                        indexList
                                  else
                                        failwithf "The instance range specified is not valid."

let evalAttributeListElement (attList:ResizeArray<string>) = function
    | AttName name -> if attList.Contains(name) then
                        [name]
                      else
                        failwithf "An attribute with name '%s' is not defined" name
    | AttIndex idx -> try
                        [attList.[idx]]
                      with
                       | :? ArgumentOutOfRangeException as e -> failwithf "An attribute with index '%d' is not defined" idx
    | AttSequence(idx1, idx2) -> let indexList = if idx1 <= idx2 then
                                                    [idx1..idx2]
                                                 else
                                                    [idx1..(-1)..idx2]
                                 try
                                    indexList |> List.map(fun idx -> attList.[idx])
                                 with
                                 | :? ArgumentOutOfRangeException as e -> failwith "The attribute range specified is not valid. 
                                                                                    An attribute with index '%d' is not defined" (Convert.ToInt32(e.ActualValue))

let evalParameterValue attList nOfInstances = function
    | Bool value -> value |> box        
    | Integer value -> value |> box
    | Double value -> value |> box      
    | String str -> str |> box
    | InstList (compl, list) -> let resList = list |> List.collect(fun el -> evalInstanceListElement nOfInstances el) |> Set.ofList |> Set.toList
                                if compl then           // Se true, la lista non è da invertire
                                    resList |> box
                                else
                                    [0..nOfInstances-1] |> List.filter(fun el -> not (List.exists(fun el2 -> el = el2) resList) ) |> box  // Filtro dalla lista "completa" ([0..nOfInstances]) tutti gli elementi che ci sono nella lista calcolata (resList)
    | AttList (compl, list) ->  let resList = list |> List.collect(fun el -> evalAttributeListElement attList el) |> Set.ofList |> Set.toList
                                if compl then
                                    resList |> box
                                else
                                    attList |> Seq.filter(fun el -> not (List.exists(fun el2 -> el = el2) resList) ) |> Seq.toList |> box

let evalParameter (store:ParameterStore) attList nOfInstances = function
    | Parameter(name, value) -> let parValue = evalParameterValue attList nOfInstances value
                                store.AddValue(name, parValue)

let evalFilter invokeFunction table attList nOfInstances = function
    | Filter(name, parameterList) ->    let rules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
                                        (filterParamterNames name) 
                                        |> List.iter(fun filterName -> rules.Add(filterName, (fun s o -> ())))          // So che i check sono fatti nel modulo Preprocessing
                                        let store = new ParameterStore(rules)

                                        // Estraggo i parametri dall'AST
                                        parameterList |> List.iter(fun par -> evalParameter store attList nOfInstances par)

                                        // Preparo il dizionario per i parametri
                                        let parameters = new Dictionary<string, obj>(HashIdentity.Structural)
                                        store.ParameterValues
                                        |> Seq.iter(fun (name, seqObj) -> parameters.Add(name, seqObj |> Seq.head))
                                        parameters.Add("table", table)

                                        // Invoco il filtro
                                        invokeFunction name parameters
                                        ()

let evalAspect (builder:Builder<'T>) attList nOfInstances = function
    | Aspect(name, parameterList) -> let store = builder.AddAspect(name)
                                     parameterList |> List.iter(fun par -> evalParameter store attList nOfInstances par)


let private directiveRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
directiveRules.Add("LOAD_NETWORK", (fun name input -> if input.GetType() <> typeof<string> then
                                                         invalidArg name "Wrong type, expected 'string'" ))

directiveRules.Add("LOAD_TRAINING", (fun name input -> if not (typeof<string>.IsInstanceOfType(input)) then
                                                                invalidArg name "Wrong type, expected 'string'" ))

let private globalRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
globalRules.Add("TRAINING_TABLE", (fun name input ->    if not (typeof<DataTable>.IsInstanceOfType(input)) then     
                                                            invalidArg name "Wrong type, expected 'DataTable'" ))

globalRules.Add("TRAINING_SET", (fun name input ->  if not (typeof<string>.IsInstanceOfType(input)) then
                                                        invalidArg name "Wrong type, expected 'string'" ))

globalRules.Add("CLASS_ATTRIBUTE", (fun name input ->   if input.GetType() <> typeof<string> then
                                                            invalidArg name "Wrong type, expected 'string'" ))
// TODO Try..with con messaggi di errore??
let evalNetwork (net:Network) : (SupervisedNeuralNetwork * DataTable * ValidationStatistics) =
    let attList = new ResizeArray<string>()
    let networkBuilderFactory = new BuilderFactory<Builder<SupervisedNeuralNetwork>, SupervisedNeuralNetwork>()
    let trainingBuilderFactory = new BuilderFactory<Builder<TrainigFunctionType>, TrainigFunctionType>()
    
    let directiveStore = new ParameterStore(directiveRules)
    let globalStore = new ParameterStore(globalRules)

    // Valuto le direttive e carico i builder
    net.Directives |> List.iter(fun par -> evalParameter directiveStore attList 0 par ) 
    directiveStore.ParameterValues |> Seq.iter(fun (name, values) -> if name = "LOAD_NETWORK" then
                                                                        values |> Seq.iter(fun value -> networkBuilderFactory.LoadBuilder(value.ToString()) |> ignore)
                                                                     elif name = "LOAD_TRAINING" then
                                                                        values |> Seq.iter(fun value -> trainingBuilderFactory.LoadBuilder(value.ToString()) |> ignore) )

    // 1 - Carico il training set
    globalStore.AddValue("TRAINING_SET", net.TrainingSet)                      
    globalStore.AddValue("CLASS_ATTRIBUTE", net.ClassAttribute)                
    let trainingSet = buildTableFromArff net.TrainingSet
    globalStore.AddValue("TRAINING_TABLE", trainingSet) 

    let nOfInstances = trainingSet.Rows.Count
    for col in trainingSet.Columns do
        attList.Add(col.ColumnName)

    // 2 - Filtraggio
    // Prima gli attributi e poi le istanze
    let attFilters, instFilters = net.Preprocessing
    attFilters |> List.iter(fun filter -> evalFilter invokeAttributeFilter trainingSet attList nOfInstances filter)
    instFilters |> List.iter(fun filter -> evalFilter invokeInstanceFilter trainingSet attList nOfInstances filter)

    // 3 - Costruzione rete
    let netName, paramList, aspectList = net.NetworkDefinition
    let netBuilder = networkBuilderFactory.CreateBuilder(netName)
    paramList |> List.iter(fun par -> evalParameter netBuilder.LocalParameters attList nOfInstances par)
    aspectList |> List.iter(fun aspect -> evalAspect netBuilder attList nOfInstances aspect )
    let NN = netBuilder.Build(globalStore)

    // 4 - Training
    let trainName, paramList, aspectList = net.Training
    let trainBuilder = trainingBuilderFactory.CreateBuilder(trainName)
    paramList |> List.iter(fun par -> evalParameter trainBuilder.LocalParameters attList nOfInstances par)
    aspectList |> List.iter(fun aspect -> evalAspect trainBuilder attList nOfInstances aspect )
    let trainAlg = trainBuilder.Build(globalStore)

    NN.TrainingFunction <- trainAlg
    NN.Train(trainingSet, net.ClassAttribute)

    // 5 - Validation
    let paramList, aspectList = net.Validation
    let valBuilder = new BasicValidationBuilder()
    paramList |> List.iter(fun par -> evalParameter valBuilder.LocalParameters attList nOfInstances par)
    aspectList |> List.iter(fun aspect -> evalAspect valBuilder attList nOfInstances aspect )
    let testTable = valBuilder.Build(globalStore)

    let stat = NN.Validate(testTable)

    (NN, trainingSet, stat)
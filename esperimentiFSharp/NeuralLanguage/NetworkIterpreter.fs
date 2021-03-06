﻿module NetworkIterpreter

open System
open AST
open Neural
open ParameterStore
open System.Collections.Generic
open Preprocessing
open Builder
open System.Data
open TableUtilities
open Validation

open System.Threading
open System.ComponentModel

open System.Windows.Forms


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
                                 | :? ArgumentOutOfRangeException as e -> failwith "The attribute range specified is not valid.\nAn attribute with index '%d' is not defined" (Convert.ToInt32(e.ActualValue))


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


let evalParameter attList nOfInstances = function
    | Parameter(name, value) -> let parValue = evalParameterValue attList nOfInstances value
                                (name, parValue)


let evalFilter invokeFunction table attList nOfInstances = function
    | Filter(name, parameterList) ->    let rules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
                                        (filterParamterNames name) 
                                        |> List.iter(fun filterName -> rules.Add(filterName, (fun s o -> ())))          // So che i check sono fatti nel modulo Preprocessing
                                        let store = new ParameterStore(rules)

                                        // Estraggo i parametri dall'AST
                                        parameterList 
                                        |> List.map(fun par -> evalParameter attList nOfInstances par) 
                                        |> List.iter (fun (name, value) -> store.AddValue(name, value))

                                        // Preparo il dizionario per i parametri
                                        let parameters = new Dictionary<string, obj>(HashIdentity.Structural)
                                        store.ParameterValues
                                        |> Seq.iter(fun (name, seqObj) -> parameters.Add(name, seqObj |> Seq.head))
                                        parameters.Add("table", table)

                                        // Invoco il filtro
                                        invokeFunction name parameters
                                        ()


let evalAspect attList nOfInstances = function
    | Aspect(aspectName, parameterList) -> let valueList = parameterList 
                                                             |> List.map(fun par -> evalParameter attList nOfInstances par)
                                                             |> List.map (fun (name, value) -> (name, value))
                                           (aspectName, valueList)

let private initDirectiveRules () =
    let directiveRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
    directiveRules.Add("LOAD_NETWORK", (fun name input -> if input.GetType() <> typeof<string> then
                                                                invalidArg name "Wrong type, expected 'string'" ))

    directiveRules.Add("LOAD_TRAINING", (fun name input -> if not (typeof<string>.IsInstanceOfType(input)) then
                                                                    invalidArg name "Wrong type, expected 'string'" ))
    directiveRules


let private initGlobalRules () =
    let globalRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
    globalRules.Add("TRAINING_TABLE", (fun name input ->    if not (typeof<DataTable>.IsInstanceOfType(input)) then     
                                                                invalidArg name "Wrong type, expected 'DataTable'" ))

    globalRules.Add("TRAINING_SET", (fun name input ->  if not (typeof<string>.IsInstanceOfType(input)) then
                                                            invalidArg name "Wrong type, expected 'string'" ))

    globalRules.Add("CLASS_ATTRIBUTE", (fun name input ->   if input.GetType() <> typeof<string> then
                                                                invalidArg name "Wrong type, expected 'string'" ))
    globalRules


let private loadTrainingSet (globalStore:ParameterStore) net =
    globalStore.AddValue("TRAINING_SET", net.TrainingSet)                      
    globalStore.AddValue("CLASS_ATTRIBUTE", net.ClassAttribute)                
    let trainingSet = buildTableFromArff net.TrainingSet
    globalStore.AddValue("TRAINING_TABLE", trainingSet) 

let private invokeFilters net filters filterInvocationFunction trainingSet attList nOfInstances = 
    filters |> List.iter(fun filter -> evalFilter filterInvocationFunction trainingSet attList nOfInstances filter)

let private buildNetwork net (networkBuilderFactory:BuilderFactory<Builder<SupervisedNeuralNetwork>, SupervisedNeuralNetwork>) attList nOfInstances globalStore =
    let netName, paramList, aspectList = net.NetworkDefinition
    let netBuilder = networkBuilderFactory.CreateBuilder(netName)
    paramList 
    |> List.map(fun par -> evalParameter attList nOfInstances par)
    |> List.iter (fun (name, value) -> netBuilder.LocalParameters.AddValue(name, value))
    aspectList 
    |> List.map(fun aspect -> evalAspect attList nOfInstances aspect )
    |> List.iter(fun (aspectName, valueList) -> let store = netBuilder.AddAspect(aspectName)
                                                valueList
                                                |> List.iter (fun (name, value) -> store.AddValue(name, value)))
    netBuilder.Build(globalStore)

let private train net (neuralNetwork:SupervisedNeuralNetwork) trainingSet (trainingBuilderFactory:BuilderFactory<Builder<TrainigFunctionType>, TrainigFunctionType>) attList nOfInstances globalStore  =
    let trainName, paramList, aspectList = net.Training
    let trainBuilder = trainingBuilderFactory.CreateBuilder(trainName)
    paramList 
    |> List.map(fun par -> evalParameter attList nOfInstances par)
    |> List.iter (fun (name, value) -> trainBuilder.LocalParameters.AddValue(name, value))
    aspectList 
    |> List.map(fun aspect -> evalAspect attList nOfInstances aspect )
    |> List.iter(fun (aspectName, valueList) -> let store = trainBuilder.AddAspect(aspectName)
                                                valueList
                                                |> List.iter (fun (name, value) -> store.AddValue(name, value)))
    let trainAlg = trainBuilder.Build(globalStore)

    neuralNetwork.TrainingFunction <- trainAlg
    neuralNetwork.Train(trainingSet, net.ClassAttribute)

let private validate net (neuralNetwork:SupervisedNeuralNetwork) attList nOfInstances globalStore =
    match net.Validation with
        | ([], []) -> None
        | (paramList,aspectList) -> let valBuilder = new BasicValidationBuilder()
                                    paramList 
                                    |> List.map(fun par -> evalParameter attList nOfInstances par)
                                    |> List.iter (fun (name, value) -> valBuilder.LocalParameters.AddValue(name, value))
                                    aspectList 
                                    |> List.map(fun aspect -> evalAspect attList nOfInstances aspect )
                                    |> List.iter(fun (aspectName, valueList) -> let store = valBuilder.AddAspect(aspectName)
                                                                                valueList
                                                                                |> List.iter (fun (name, value) -> store.AddValue(name, value)))
                                    let testTable = valBuilder.Build(globalStore)
                                    Some(neuralNetwork.Validate(testTable))

let private loadBuilders net (directiveStore:ParameterStore) attList =
    let networkBuilderFactory = new BuilderFactory<Builder<SupervisedNeuralNetwork>, SupervisedNeuralNetwork>()
    let trainingBuilderFactory = new BuilderFactory<Builder<TrainigFunctionType>, TrainigFunctionType>()
    net.Directives 
    |> List.map(fun par -> evalParameter attList 0 par ) 
    |> List.iter (fun (name, value) -> directiveStore.AddValue(name, value))
    directiveStore.ParameterValues |> Seq.iter(fun (name, values) -> if name = "LOAD_NETWORK" then
                                                                        values |> Seq.iter(fun value -> networkBuilderFactory.LoadBuilder(value.ToString()) |> ignore)
                                                                     elif name = "LOAD_TRAINING" then
                                                                        values |> Seq.iter(fun value -> trainingBuilderFactory.LoadBuilder(value.ToString()) |> ignore) )
    (networkBuilderFactory, trainingBuilderFactory)


// http://stackoverflow.com/questions/5244656/how-to-handle-errors-during-parsing-in-f
let evalNetwork (net:Network) : (SupervisedNeuralNetwork * DataTable * ValidationStatistics option * (SupervisedNeuralNetwork -> Control)) =
    let attList = new ResizeArray<string>()
    
    let directiveStore = new ParameterStore(initDirectiveRules ())
    let globalStore = new ParameterStore(initGlobalRules ())

    // Valuto le direttive e carico i builder
    let networkbuilderFactory, trainingBuilderFactory = loadBuilders net directiveStore attList

    // 1 - Carico il training set
    loadTrainingSet globalStore net

    let trainingSet = globalStore.GetValues("TRAINING_TABLE") |> Seq.head :?> DataTable

    let nOfInstances = trainingSet.Rows.Count
    for col in trainingSet.Columns do
        attList.Add(col.ColumnName)

    // 2 - Filtraggio (Pesante!!!)
    // Prima gli attributi e poi le istanze
    let attFilters, instFilters = net.Preprocessing
    invokeFilters net attFilters invokeAttributeFilter trainingSet attList nOfInstances
    invokeFilters net instFilters invokeInstanceFilter trainingSet attList nOfInstances
    

    // 3 - Costruzione rete
    let NN = buildNetwork net networkbuilderFactory attList nOfInstances globalStore

    // 4 - Training (Pesante!!!)
    train net NN trainingSet trainingBuilderFactory attList nOfInstances globalStore

    // 5 - Validation (Pesante!!!)
    let stat = validate net NN attList nOfInstances globalStore

    let netName, _, _ = net.NetworkDefinition
    let builder = networkbuilderFactory.CreateBuilder(netName)
    let visualizer = builder.GetVisualizer
                       
    (NN, trainingSet, stat, visualizer)


// Get a BackgroundWorker configured to evaluate the source code
// The worker supports cancellation and reporting of progress
let getEvalNetworkWorker =
    let worker = new BackgroundWorker()
    worker.WorkerSupportsCancellation <- true
    worker.WorkerReportsProgress <- true

    worker.DoWork.Add(fun args ->
                            let net = args.Argument :?> Network
                            let attList = new ResizeArray<string>()
    
                            let directiveStore = new ParameterStore(initDirectiveRules ())
                            let globalStore = new ParameterStore(initGlobalRules ())

                            // Valuto le direttive e carico i builder
                            let networkbuilderFactory, trainingBuilderFactory = loadBuilders net directiveStore attList

                            // 1 - Carico il training set
                            loadTrainingSet globalStore net
                            
                            worker.ReportProgress(10);
                            if worker.CancellationPending = true then
                                args.Cancel <- true
                                ()

                            let trainingSet = globalStore.GetValues("TRAINING_TABLE") |> Seq.head :?> DataTable

                            let nOfInstances = trainingSet.Rows.Count
                            for col in trainingSet.Columns do
                                attList.Add(col.ColumnName)

                            // 2 - Filtraggio (Pesante!!!)
                            // Prima gli attributi e poi le istanze
                            let attFilters, instFilters = net.Preprocessing
                            invokeFilters net attFilters invokeAttributeFilter trainingSet attList nOfInstances
                            worker.ReportProgress(30);
                            if worker.CancellationPending = true then
                                args.Cancel <- true
                                ()

                            invokeFilters net instFilters invokeInstanceFilter trainingSet attList nOfInstances
                            worker.ReportProgress(50);
                            if worker.CancellationPending = true then
                                args.Cancel <- true
                                ()
    

                            // 3 - Costruzione rete
                            let NN = buildNetwork net networkbuilderFactory attList nOfInstances globalStore
                            worker.ReportProgress(60);
                            if worker.CancellationPending = true then
                                args.Cancel <- true
                                ()

                            // 4 - Training (Pesante!!!)
                            train net NN trainingSet trainingBuilderFactory attList nOfInstances globalStore
                            worker.ReportProgress(80);
                            if worker.CancellationPending = true then
                                args.Cancel <- true
                                ()

                            // 5 - Validation (Pesante!!!)
                            let stat = validate net NN attList nOfInstances globalStore
                            worker.ReportProgress(100);
                            if worker.CancellationPending = true then
                                args.Cancel <- true
                                ()

                            let netName, _, _ = net.NetworkDefinition
                            let builder = networkbuilderFactory.CreateBuilder(netName)
                            let visualizer = builder.GetVisualizer
                       
                            args.Result <- (NN, trainingSet, stat, visualizer)
                            )      

    worker
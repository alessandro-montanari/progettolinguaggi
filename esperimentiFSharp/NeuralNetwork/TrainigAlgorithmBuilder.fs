module TrainigAlgorithmBuilder

open System
open System.Data
open System.Collections.Generic
open Parameter
open Neural
open NeuralTypes

type BackPropagationBuilder() =
    
    let initParameterTypes() =                                               
        let dic = new Dictionary<string, Type>(HashIdentity.Structural)
        dic.Add("MOMENTUM", Number(0.0).GetType())          
        dic.Add("LEARNIING_RATE", Number(0.0).GetType())
        dic.Add("EPOCHS", Number(0.0).GetType())   
        dic

    let _paramStore = new ParameterStore(initParameterTypes())

    member this.ParameterStore = _paramStore

    member this.BuildTrainingFunction() : SupervisedNeuralNetwork -> DataTable -> string -> unit =
       
        (fun a b c -> ())

let der (f:double->double) (x:double) =
    (f(x-2.0) - 8.0*(f(x-1.0)) + 8.0*(f(x+1.0)) - f(x+2.0)) / 12.0

//let der2 (f:double->double) (x:double) =
//    let dx = sqrt epsilon_float
//    (f(x+dx) - f(x-dx)) / (2.0*dx)

let func2 x = x**3.0-x-1.0

let derSigmoid x  = (exp(x)) / (exp(x)+1.0)**2.0

let learningRate = 0.3

// nomVal è uno dei valori nominali a cui è associato il neurone passato come primo argomento TODO DA FARE OPZIONALE
let diffOutputs (neuron:Neuron) (nomVal:string) (expValue:AttributeValue) =
    if expValue.IsNumber then
        let expectedOutput = match expValue with | Numeric(value) -> value
        (expectedOutput - neuron.Output)
    else
        let expectedOutput = match expValue with | Nominal(value,_) -> value
        match expectedOutput with
        | nomVal -> 1.0 - neuron.Output
        | _ -> 0.0 - neuron.Output

let currentError (attName:string) (expValue:AttributeValue) (outputLayer:Dictionary<string, Neuron>) =
    if outputLayer.Keys.Count = 1 then                         // L'attributo da classificare è numerico
        let expectedOutput = match expValue with | Numeric(value) -> value
        (diffOutputs outputLayer.[attName] "" expValue)**2.0
    else                                                        // L'attributo da classificare è nominale
        outputLayer
            |> Seq.map (fun el -> (diffOutputs el.Value el.Key expValue)**2.0 )
            |> Seq.sum

let backPropagation (nn:SupervisedNeuralNetwork) (dt:DataTable) (attName:string) =
    //TODO skippare le righe che hanno l'attName Missing
    let nn = if nn.GetType() = typeof<MultiLayerNetwork> then
                nn :?> MultiLayerNetwork
             else 
                failwithf "the back propagation algorithm cannot be applied to a network of type %A" (nn.GetType())
    let globalError = 0.0
    for row in dt.Rows do
        nn.Activate(row)
        let expectedValue = row.[attName] :?> AttributeValue
        let currentError = currentError attName expectedValue nn.OutputLayer

        // Assumo rete completamente complesse
        // Calcolo i delta dello strato d'uscita
        let deltaDictionaryOutput = new Dictionary<Neuron, double>(HashIdentity.Structural)
        nn.OutputLayer
            |> Seq.map (fun el -> ((diffOutputs el.Value el.Key expectedValue), el.Value))
            |> Seq.map (fun (diff, neur) -> let netj = neur.InputMap           
                                                        |> Seq.map (fun el -> (el.Key.Output, el.Value))
                                                        |> neur.ActivationFunction
                                            deltaDictionaryOutput.Add(neur, (diff*(der neur.OutputFunction netj)) ) ) |> ignore
        
        // Calcolo i delta degli strati nascosti
        let deltaDictionaryHiddenLayers = new Dictionary<NeuralLayer, Dictionary<Neuron, double>>(HashIdentity.Structural)
        for layer in nn.HiddenLayers do
            layer
            |> Seq.map (fun neur ->  let deltaDictionaryHidden = new Dictionary<Neuron, double>(HashIdentity.Structural)
                                     let net = neur.InputMap
                                                |> Seq.map (fun el -> (el.Key.Output, el.Value))
                                                |> neur.ActivationFunction
                                     let previousDict = if nn.HiddenLayers.IndexOf(layer) = 0 then
                                                            deltaDictionaryOutput
                                                        else
                                                            deltaDictionaryHiddenLayers.[layer]
                                     let sum = previousDict
                                                |> Seq.sumBy (fun el -> el.Value*el.Key.InputMap.[neur])
                                     deltaDictionaryHidden.Add(neur, ((der neur.OutputFunction net)*sum))
                                     deltaDictionaryHiddenLayers.Add(layer, deltaDictionaryHidden) ) |> ignore

        
    ()


//|> Seq.map (fun (deltaj, neur) -> neur.InputMap
//                                |> Seq.map (fun el -> neur.InputMap.[el.Key] <- learningRate*deltaj*el.Key.Output) ) |> ignore
    


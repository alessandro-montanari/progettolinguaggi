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

let derSigmoid x  = (exp(x)) / (exp(x)+1.0)**2.0

let currentError (attName:string) (expValue:AttributeValue) (outputLayer:Dictionary<string, Neuron>) =
    if outputLayer.Keys.Count = 1 then                         // L'attributo da classificare è numerico
        let expectedOutput = match expValue with | Numeric(value) -> value
        (expectedOutput - outputLayer.[attName].Output)**2.0
    else
        let expectedOutput = match expValue with | Nominal(value,_) -> value
        outputLayer.Keys
            |> Seq.map (fun key -> (key, outputLayer.[key].Output) )
            |> Seq.map (fun (key, actualOutput) -> match key with
                                                    | expectedOutput -> (1.0 - actualOutput)**2.0
                                                    | _ -> (0.0 - actualOutput)**2.0 )
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
        let currentError = currentError attName row.[attName] nn.OutputLayer

    ()
    


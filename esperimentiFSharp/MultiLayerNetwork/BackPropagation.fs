module BackPropagation

open System
open System.Data
open System.Collections.Generic
open Neural
open NeuralTypes
open TableUtilities
open Builder
open MultiLayerNetwork
open Parameter

let private der (f:double->double) (x:double) =
    (f(x-2.0) - 8.0*(f(x-1.0)) + 8.0*(f(x+1.0)) - f(x+2.0)) / 12.0

// nomVal è uno dei valori nominali a cui è associato il neurone passato come primo argomento
let private diffOutputs (neuron:Neuron) (nomVal:string) (expValue:AttributeValue) =
    if expValue.IsNumber then
        let expectedOutput = expValue.NumberOf
        (expectedOutput - neuron.Output)
    else
        let expectedOutput = match expValue with | Nominal(value,_) -> value
        if expectedOutput = nomVal then
            1.0 - neuron.Output
        else
            0.0 - neuron.Output

let private currentError (attName:string) (expValue:AttributeValue) (outputLayer:Dictionary<string, Neuron>) =
    if outputLayer.Keys.Count = 1 then                         // L'attributo da classificare è numerico
        (diffOutputs outputLayer.[attName] "" expValue)**2.0
    else                                                        // L'attributo da classificare è nominale
        outputLayer
            |> Seq.map (fun el -> (diffOutputs el.Value el.Key expValue)**2.0 )
            |> Seq.sum

let backPropagation learningRate ephocs (nn:SupervisedNeuralNetwork) (dt:DataTable) (attName:string) =
    let nn = if nn.GetType() = typeof<MultiLayerNetwork> then
                nn :?> MultiLayerNetwork
             else 
                failwithf "the back propagation algorithm cannot be applied to a network of type %A" (nn.GetType())
     
    for _ in 0..ephocs do                
        let mutable globalError = 0.0
        for row in dt.Rows do
            let index = dt.Columns.IndexOf(attName)
            let expectedValue = toAttributeValue row index
            let missing = match expectedValue with 
                            | Missing -> true
                            | _ -> false
            if missing = false then                 // skippo le righe che hanno l'attName Missing
                nn.Activate(row)

                let currentError = currentError attName expectedValue nn.OutputLayer

                // Assumo rete completamente complesse
                // Calcolo i delta dello strato d'uscita
                let deltaDictionaryOutput = new Dictionary<Neuron, double>(HashIdentity.Structural)
                nn.OutputLayer
                    |> Seq.map (fun el -> ((diffOutputs el.Value el.Key expectedValue), el.Value))
                    |> Seq.iter (fun (diff, neur) -> let netj = neur.InputMap           
                                                                |> Seq.map (fun el -> (el.Key.Output, el.Value))
                                                                |> neur.ActivationFunction
                                                     deltaDictionaryOutput.Add(neur, (diff*(der neur.OutputFunction netj)) ) )
        
                // Calcolo i delta degli strati nascosti
                
                // Per ogni layer salvo il delta associato ad ogni neurone
                let deltaDictionaryHiddenLayers = new Dictionary<NeuralLayer, Dictionary<Neuron, double>>(HashIdentity.Structural)
                let hiddenLayers = nn.HiddenLayers
                                    |> Seq.toList
                                    |> List.rev
                let mutable previousDict = deltaDictionaryOutput    // sto analizzando l'ultimo hidden layer quindi lui davanti a se ha l'output layer
                for layer in hiddenLayers do
                    let deltaDictionaryHidden = new Dictionary<Neuron, double>(HashIdentity.Structural)
                    for neur in layer do
                        let net = neur.InputMap
                                        |> Seq.map (fun el -> (el.Key.Output, el.Value))
                                        |> neur.ActivationFunction
                        let sum = previousDict
                                    |> Seq.sumBy (fun el -> el.Value*el.Key.InputMap.[neur])
                        deltaDictionaryHidden.Add(neur, ((der neur.OutputFunction net)*sum))
                    previousDict <- deltaDictionaryHidden
                    deltaDictionaryHiddenLayers.Add(layer, deltaDictionaryHidden)

                // Aggiornameto pesi layer di uscita
                for neur in deltaDictionaryOutput.Keys do
                    for precNeur in Seq.toArray neur.InputMap.Keys do
                        neur.InputMap.[precNeur] <- (neur.InputMap.[precNeur]+(learningRate*deltaDictionaryOutput.[neur]*precNeur.Output))

                // Aggiornamento pesi layer nascosti
                for dic in deltaDictionaryHiddenLayers.Values do
                    for neur in dic.Keys do
                        for precNeur in Seq.toArray neur.InputMap.Keys do
                            neur.InputMap.[precNeur] <- (neur.InputMap.[precNeur]+(learningRate*dic.[neur]*precNeur.Output))
               
                globalError <- globalError+currentError


// ----------------------------------------------------------------------------------------
// -------------------------------BUILDER--------------------------------------------------
// ----------------------------------------------------------------------------------------

let private globalRules = new Dictionary<string, (string->obj->unit)>(HashIdentity.Structural)
globalRules.Add("LEARNING_RATE", (fun name input -> if input.GetType() <> typeof<double> then
                                                        invalidArg name "Wrong type, expected 'double'"
                                                    let value = unbox input
                                                    if (value < 0.0) || (value > 1.0) then
                                                        invalidArg name "Must be in the range [0; 1]" ))

globalRules.Add("EPOCHS", (fun name input -> if input.GetType() <> typeof<int> then
                                                invalidArg name "Wrong type, expected 'int'"
                                             let value = unbox input
                                             if value <= 0 then
                                                invalidArg name "Must be grater than 0" ))

let private aspectsRules = new Dictionary<string, Dictionary<string,(string->obj->unit)>>(HashIdentity.Structural)

type BackPropagationBuilder() =
    inherit Builder<TrainigFunctionType>(globalRules, aspectsRules)

    let check (builder:Builder<'T>) =
        let numOfAspects = builder.Aspects |> Seq.length
        let numOfLocParams = builder.LocalParameters.ParameterValues |> Seq.length
        let numOfLearningRate = builder.LocalParameters.GetValues("LEARNING_RATE") |> Seq.length
        let numOfEpochs = builder.LocalParameters.GetValues("EPOCHS") |> Seq.length
        if numOfAspects > 0 then
            failwith "It is not possible to set aspects in the BackPropagationBuilder"
        if numOfLocParams <> 2 then
            failwith "Two global parameters must be set in BackPropagationBuilder, LEARNING_RATE and EPOCHS"
        if numOfLearningRate <> 1 then
            failwith "Only one LEARNING_RATE parameter can be setted in BackPropagationBuilder"
        if numOfEpochs <> 1 then
            failwith "Only one EPOCHS parameter can be setted in BackPropagationBuilder"

    override this.Name = "BackPropagation"
    override this.Build(gobalParameters:ParameterStore) = 
        check this
        let rate = unbox(this.LocalParameters.GetValues("LEARNING_RATE") |> Seq.exactlyOne)
        let epochs = unbox( this.LocalParameters.GetValues("EPOCHS") |> Seq.exactlyOne)
        backPropagation rate epochs

    override this.GetVisualizer(param) =
        null

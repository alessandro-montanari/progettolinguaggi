﻿module Neural

open System
open System.Data
open System.IO
open System.Collections.Generic

// In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

//TODO DEVE ESSERE PRIVATO
let getUniqueId : (unit -> int) =
    let random = new System.Random()   
    let id = ref 0
    (function _ ->  id := !id+1
                    !id )

type ActivationFunType = seq<double * double> -> double   // input * peso
type OutputFunType = double -> double

// Funzioni di uscita
let sigmoid (t : double) : double = 1.0 / (1.0 + exp(-t))
let heavisied (t :double) (theta :double) : double =    if t < theta then
                                                            0.0
                                                        else
                                                            1.0

// Funzioni di attivazione
let sumOfProducts (input : seq<double * double> ) : double = input 
                                                                |> Seq.fold (fun acc el -> match el with (a,b) -> acc + a*b) 0.0 
                        

// Con questa modellazione di neurone posso creare qualsiasi struttura
// Comunque mi limito a supportare principlamente l'addestramento di tipo supervisionato in cui si ha sempre un trainig set d'ingresso
// con gli esempi e l'uscita desiderata

//TODO Come gestire gli input diretti (non da altri neuroni)
type Neuron(inMap : Dictionary<Neuron, double>, actFun : ActivationFunType, outFun : OutputFunType) =

    let mutable _output : double = 0.0
    
    member val Id = getUniqueId() with get
    member val ActivationFunction = actFun with get, set            // Possibilità di cambiare a runtime le funzioni
    member val OutputFunction = outFun with get, set
    member n.Output = _output
    member n.InputMap = inMap

    member n.Activate() =
        let newOutput = inMap
                        |> Seq.map (fun el -> (el.Key.Output, el.Value))
                        |> n.ActivationFunction
                        |> n.OutputFunction
        _output <- newOutput
        newOutput

    override n.Equals(aNeuron) =  
        match aNeuron with
        | :? Neuron as yN -> n.Id = yN.Id
        | _ -> false

    override n.GetHashCode() = hash n.Id

    interface System.IComparable with
        member n.CompareTo(aNeuron) =
            match aNeuron with
            | :? Neuron as yN -> compare n.Id yN.Id
            | _ -> invalidArg "aNeuron" "cannot compare values of different types"

let inline equalsOn f x (yobj : obj) =
    match yobj with
    | :? 'T as y -> (f x = f y)
    | _ -> false

type  OutputValue =
    | Numeric of double
    | Nominal of string
    
    static member Create(value : double) = Numeric(value)
    static member Create(value : string) = Nominal(value)
    static member Create(value : obj) =
        let valueType = value.GetType()
        if valueType = typeof<double> then
            Numeric(Convert.ToDouble(value))
        elif valueType = typeof<string> then
            Nominal(value.ToString())
        else
            failwithf "The type '%A' is not supported as OutputValue" valueType 

type ValidationStatistics() =
    
    let mutable _nExps = 0
    let mutable _nMiss = 0
    let mutable _nCorr = 0
    let mutable _missExps = new ResizeArray<DataRow * OutputValue>()
    let mutable _corrExps = new ResizeArray<DataRow * OutputValue>()

    member stat.NumberOfExamples = _nExps
    member stat.NumberOfMissclassifiedExamples = _nMiss
    member stat.NumberOfCorrectlyClassifiedExamples = _nCorr
    member stat.MissclassifiedExamples = _missExps.AsReadOnly()
    member stat.CorrectlyClassifiedExamples = _corrExps.AsReadOnly()

    member stat.CollectStatistics(positive, example) =
        _nExps <- stat.NumberOfExamples+1
        if positive = true then
            _nCorr <- stat.NumberOfCorrectlyClassifiedExamples+1
            _corrExps.Add(example)
        else
            _nMiss <- stat.NumberOfMissclassifiedExamples+1
            _missExps.Add(example)

[<AbstractClass>]
type SupervisedNeuralNetwork(trainingFun : TrainigFunctionType) =
    
    let mutable _classAtt = ""

    member nn.TrainedAttribute = _classAtt
    member val TrainingFunction = trainingFun with get, set
    
    member nn.Train(trainingSet : DataTable, classAtt : string) =
        _classAtt <- classAtt
        trainingFun nn trainingSet classAtt

    member nn.TrainFromArff(trainingSetPath : string, classAtt : string) = 
        let table = TableUtilities.buildTableFromArff trainingSetPath      
        nn.Train(table, classAtt)

    // Come fare per i parametri -> ci sarà un sorta di builder che genera il testSet in base ai parametri
    member nn.Validate(testTable : DataTable) : ValidationStatistics =       
        let stat = new ValidationStatistics()
        let cols = seq{ for colName in testTable.Columns -> colName.ColumnName }    // costruisco l'array di colonne che mi servono (non c'è quella del classAtt)
                    |> Seq.filter (fun name -> name <> _classAtt)
                    |> Array.ofSeq
        let testTable = testTable.DefaultView.ToTable("testTable", false, cols)
        let expectedTable = testTable.DefaultView.ToTable("expectedTable", false, [| _classAtt |]) 
        for testRow in testTable.Select() do
            for expRow in expectedTable.Select() do
                let outputValue = nn.Classify testRow
                if outputValue = OutputValue.Create(expRow.[0]) then        // L'operatore '=' funziona anche sulle Discriminated Unions
                    stat.CollectStatistics(true, (testRow, outputValue))
                else
                     stat.CollectStatistics(false, (testRow, outputValue))
        stat

    member nn.ValidateFromArff(testSetPath : string) : ValidationStatistics =
        let table = TableUtilities.buildTableFromArff testSetPath 
        nn.Validate(table)

    // Classifica in base all'attributo specificato in fase di training
    // La DataRow in ingresso ovviamente non deve contenere l'attributo da classificare
    // Astratto perché solo le reti concrete sanno come classificare un'istanza
    abstract member Classify : DataRow -> OutputValue   //TODO nome diverso da Classify (Pulse o qualcosa di simile)

and TrainigFunctionType = SupervisedNeuralNetwork -> DataTable -> string -> unit      // Modifica la rete passata come primo parametro


// Per il preprocessing ci sarà "qualcosa" che prende i filtri e man mano li applica e alla fine sputa fuori una
// nuova DataTable (quella originale si perde, per efficienza)


//let layer1 = [ for i in 0 .. 9 -> new Neuron(3, sumOfProducts, sigmoid) ]
//let layer2 = [ for i in 0 .. 4 -> new Neuron(10, sumOfProducts, sigmoid) ]
//let layer3 = [ new Neuron(5, sumOfProducts, sigmoid) ]
//let inputs = [ 0.5; 0.000099; 0.06 ];;
//
//let outputsLayer1 = layer1 
//                        |> List.map (fun n -> n.Activate(inputs))
//
//let outputsLayer2 = layer2
//                        |> List.map (fun n -> n.Activate(outputsLayer1))
//
//let outputLayer3 = layer3
//                    |> List.map (fun n -> n.Activate(outputsLayer2))

    




open System.Data
open System.IO
open System.Collections.Generic

// In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

let random = new System.Random()    // deve essre in qualche modo statico

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
type Neuron(inMap : Dictionary<Neuron, double>, actFun : ActivationFunType, outFun : OutputFunType) =

    let mutable _output : double = 0.0
    
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


type  OutputValue =
        | Numeric of double
        | Nominal of string

type ValidationStatistics() =
    
    member val NumberOfExamples = 0 with get, set
    member val NumberOfMissclassifiedExamples = 0 with get, set
    member val NumberOfCorrectlyClassifiedExamples = 0 with get, set
    member val MissclassifiedExamples = seq<(DataRow * OutputValue)> with get, set
    member val CorrectlyClassifiedExamples = seq<(DataRow * OutputValue)> with get, set

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

    member nn.Validate(testTable : DataTable) : ValidationStatistics =       // Li posso già implementare invocando Classify
        let stat = new ValidationStatistics()
        let cols = seq{ for colName in testTable.Columns -> colName.ColumnName }    // costruisco l'array di colonne che mi servono (non c'è quella del classAtt)
                    |> Seq.filter (fun name -> name <> _classAtt)
                    |> Array.ofSeq
        let testTable = testTable.DefaultView.ToTable("testTable", false, cols)
        let expectedTable = testTable.DefaultView.ToTable("expectedTable", false, [| _classAtt |])
        testTable.Select()
            |> Seq.iter2 (fun testRow expRow -> 
                                            let outputValue = nn.Classify testRow
                                            let output : 'T = match outputValue with 
                                                                | Numeric(v) -> v
                                                                | Nominal(v) -> v
                                            if output = expRow.[0] then
                                                stat.NumberOfCorrectlyClassifiedExamples <- stat.NumberOfCorrectlyClassifiedExamples+1
                                                stat.CorrectlyClassifiedExamples <- Seq.append seq[(testRow, outputValue)] stat.CorrectlyClassifiedExamples ) expectedTable.Select() 
        stat

    member nn.ValidateFromArff(testSetPath : string) : ValidationStatistics =
        let table = TableUtilities.buildTableFromArff testSetPath 
        new ValidationStatistics(table)

    // Classifica in base all'attributo specificato in fase di training
    // Ritorna Numeric o Nominal in base all'attributo scelto per il training
    // La DataRow in ingresso ovviamente non deve contenere l'attributo da classificare
    abstract member Classify : DataRow -> OutputValue

and TrainigFunctionType = SupervisedNeuralNetwork -> DataTable -> string -> unit      // Modifica la rete passata come primo parametro






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

    




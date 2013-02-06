module Neural

open System
open System.Data
open System.IO
open System.Collections.Generic
open NeuralTypes
open TableUtilities

//TODO In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

let private getUniqueId : (unit -> int) =
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
let linear (t : double) : double = t                    // utile per predire valori numerici

// Funzioni di attivazione
let sumOfProducts (input : seq<double * double> ) : double = input 
                                                                |> Seq.fold (fun acc el -> match el with (a,b) -> acc + a*b) 0.0 
                        

// Con questa modellazione di neurone posso creare qualsiasi struttura
// Comunque mi limito a supportare principlamente l'addestramento di tipo supervisionato in cui si ha sempre un trainig set d'ingresso
// con gli esempi e l'uscita desiderata
type Neuron(inMap : Dictionary<Neuron, double>, actFun : ActivationFunType, outFun : OutputFunType) =

    let mutable _output : double = 0.0
    
    member val Id = getUniqueId() with get
    member val ActivationFunction = actFun with get, set            // Possibilità di cambiare a runtime le funzioni
    member val OutputFunction = outFun with get, set
    abstract Output : double with get                               // Abstract così che ConstantNeuron possa ridefinirlo (non posso mettere _output protected -> non esiste protected!)
    default n.Output = _output
    member n.InputMap = inMap

    member n.Activate() =
        _output <- inMap
                        |> Seq.map (fun el -> (el.Key.Output, el.Value))    // Creo una sequence di tuple di double con l'uscita di neuroni a monte e i pesi sulle connessioni
                        |> n.ActivationFunction
                        |> n.OutputFunction
        _output

    override n.Equals(aNeuron) =                                            // Ridefinisco Equals e GetHashCode per poter usare un Neurone come chiave del dizionario in InputMap
        match aNeuron with
        | :? Neuron as yN -> n.Id = yN.Id
        | _ -> false

    override n.GetHashCode() = hash n.Id

    interface System.IComparable with
        member n.CompareTo(aNeuron) =
            match aNeuron with
            | :? Neuron as yN -> compare n.Id yN.Id
            | _ -> invalidArg "aNeuron" "cannot compare values of different types"

    new() = Neuron(new Dictionary<Neuron, double>(), sumOfProducts, (fun x -> x))       // Costruisce un neurone non collegato a nessuno con funzione di uscita pari all'identità

// Rappresenta un neurone con uscita fissa e modificabile. Fissa nel senso che non dipende da altri neuroni.
// Di solito utilizzato per i neuroni di input
type ConstantNeuron() =
    inherit Neuron()

    let mutable _output = 0.0

    override n.Output = _output

    member n.SetOutput(out : double) =
        _output <- out

// ==================================================================================================================================================
// ==================================================================================================================================================


/// Generic container of statistics
type ValidationStatistics(dict : Dictionary<string,obj>) =
    
    let _statDict = dict

    member vs.StatisticNames = _statDict.Keys |> Seq.readonly
    member vs.StatisticValues = _statDict.Values |> Seq.readonly
    member vs.Statistics = _statDict |> Seq.readonly

    member vs.PrintStatistcs() =
        vs.PrintStatistcs(_statDict.Keys |> Seq.toList)

     member vs.PrintStatistcs(stats:string list) =
        _statDict
        |> Seq.filter (fun el -> List.exists (fun stat -> el.Key = stat) stats)
        |> Seq.iter (fun el -> printfn "%s : %A" el.Key el.Value)

// Interfaccia
type private StatisticsCollector =
    abstract member Init : string -> unit
    abstract member CollectStatistics : AttributeValue*AttributeValue -> unit
    abstract member BuildValidationStatistics : unit -> ValidationStatistics
 
type private NumericStatisticsCollector() =

    let predictions = new ResizeArray<double*double*double>()  // Esempio, valore predetto, errore
    let mutable attName = ""
    let mutable _nExps = 0

    interface StatisticsCollector with
        member nc.Init(att:string) =
            attName <- att
        
        member nc.CollectStatistics(predicted:AttributeValue, actual:AttributeValue) =
            _nExps <- _nExps+1
            let actualValue = actual.NumberOf
            let predictedValue = predicted.NumberOf
            let error = actualValue-predictedValue
            predictions.Add(actualValue, predictedValue, error)

        member nc.BuildValidationStatistics() =
            let dict = new Dictionary<string,obj>(HashIdentity.Structural)

            dict.Add("Number of Examples", _nExps)

            let MeanAbsoluteError = (predictions |> Seq.sumBy (fun (_,_,error) -> abs(error))) / double predictions.Count
            dict.Add("Mean Absolute Error", MeanAbsoluteError)

            let SumSquaredError = predictions |> Seq.sumBy (fun (_,_,error) -> error*error) 
            let MeanSquaredError = SumSquaredError / double predictions.Count
            dict.Add("Mean Squared Error", MeanSquaredError)

            let RootMeanSquaredError = sqrt(MeanSquaredError)
            dict.Add("Root Mean Squared Error", RootMeanSquaredError)
                
            new ValidationStatistics(dict)

type private NominalStatisticsCollector() =

    let predictions = new ResizeArray<string*string>()  // Esempio, valore predetto
    let mutable attName = ""
    let mutable _nExps = 0
    let mutable _nMiss = 0
    let mutable _nCorr = 0
    let mutable _missExps = new ResizeArray<DataRow * AttributeValue>()
    let mutable _corrExps = new ResizeArray<DataRow * AttributeValue>()
        
    interface StatisticsCollector with
        member nc.Init(att:string) =
            attName <- att
        
        member nc.CollectStatistics(predicted:AttributeValue, actual:AttributeValue) =
            _nExps <- _nExps+1
            let actualValue = match actual with
                                | Nominal (s,_) -> s
            let predictedValue = match predicted with
                                | Nominal (s,_) -> s

            predictions.Add(actualValue, predictedValue)
            if actualValue = predictedValue then
                _nCorr <- _nCorr+1
            else
                _nMiss <- _nMiss+1

        member nc.BuildValidationStatistics() =
            let dict = new Dictionary<string,obj>(HashIdentity.Structural)
           
            dict.Add("Number of Examples", _nExps)

            dict.Add("Number of Correctly Classified Instances", _nCorr)
            dict.Add("Number of Incorrectly Classified Instances", _nMiss)
            dict.Add("Percentage of Correctly Classified Examples", ((Convert.ToDouble(_nCorr)/Convert.ToDouble(_nExps))*100.0))
            dict.Add("Percentage of Incorrectly Classified Examples", ((Convert.ToDouble(_nMiss)/Convert.ToDouble(_nExps))*100.0))
                
            new ValidationStatistics(dict)
            
[<AbstractClass>]
type SupervisedNeuralNetwork(trainingFun : TrainigFunctionType) =
    
    let mutable _classAtt = ""
    let mutable _trainingTable = null

    member nn.TrainedAttribute = _classAtt
    member val TrainingFunction = trainingFun with get, set
    member nn.TrainingTable = _trainingTable
    
    member nn.Train(trainingSet : DataTable, classAtt : string) =
        _classAtt <- classAtt
        _trainingTable <- trainingSet
        nn.TrainingFunction nn trainingSet classAtt

    member nn.TrainFromArff(trainingSetPath : string, classAtt : string) = 
        let table = TableUtilities.buildTableFromArff trainingSetPath      
        nn.Train(table, classAtt)

    member nn.Validate(testTableIn : DataTable) : ValidationStatistics =       
        let stat : StatisticsCollector  = match (_trainingTable.Columns.[_classAtt] :?> AttributeDataColumn).AttributeType with
                                            | AttributeType.Numeric -> new NumericStatisticsCollector() :> StatisticsCollector
                                            | AttributeType.Nominal _ -> new NominalStatisticsCollector() :> StatisticsCollector
                                            | _ -> failwith "The multilayer network can only be used for predict numeric or nominal attributes"
        stat.Init(_classAtt)
        let index = testTableIn.Columns.IndexOf(_classAtt)

        for i in 0..(testTableIn.Rows.Count-1) do
            let testRow = testTableIn.Rows.[i]
            let expectedValue = toAttributeValue testRow index
            let predictedValue = nn.Classify testRow  
            stat.CollectStatistics(predictedValue, expectedValue)

        stat.BuildValidationStatistics()

    member nn.ValidateFromArff(testSetPath : string) : ValidationStatistics =
        let table = TableUtilities.buildTableFromArff testSetPath 
        nn.Validate(table)

    // Classifica in base all'attributo specificato in fase di training
    // Astratto perché solo le reti concrete sanno come classificare un'istanza
    abstract member Classify : DataRow -> AttributeValue 

and TrainigFunctionType = SupervisedNeuralNetwork -> DataTable -> string -> unit      // Modifica la rete passata come primo parametro
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
    abstract Output : double with get
    default n.Output = _output
    member n.InputMap = inMap

    member n.Activate() =
        _output <- inMap
                        |> Seq.map (fun el -> (el.Key.Output, el.Value))    // Creo una sequence di tuple di double con l'uscita di neuroni a monte e i pesi sulle connessioni
                        |> n.ActivationFunction
                        |> n.OutputFunction
        _output

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
            
//            dict.Add("Examples (actual, predicted, error)", predictions |> Seq.readonly |> Seq.toList)
                
            new ValidationStatistics(dict)

type private NominalStatisticsCollector() =

    let predictions = new ResizeArray<string*string>()  // Esempio, valore predetto, errore
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
//            dict.Add("Examples (actual, predicted)", predictions |> Seq.readonly |> Seq.toList)
                
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
        trainingFun nn trainingSet classAtt

    member nn.TrainFromArff(trainingSetPath : string, classAtt : string) = 
        let table = TableUtilities.buildTableFromArff trainingSetPath      
        nn.Train(table, classAtt)

    member nn.Validate(testTableIn : DataTable) : ValidationStatistics =       
        let stat : StatisticsCollector  = match (_trainingTable.Columns.[_classAtt] :?> AttributeDataColumn).AttributeType with
                                            | AttributeType.Numeric -> new NumericStatisticsCollector() :> StatisticsCollector
                                            | AttributeType.Nominal _ -> new NominalStatisticsCollector() :> StatisticsCollector
                                            | _ -> failwith "The multilayer network can only be used for predict numeric or nominal attributes"
        stat.Init(_classAtt)
        
        for i in 0..(testTableIn.Rows.Count-1) do
            let testRow = testTableIn.Rows.[i]
            let index = testTableIn.Columns.IndexOf(_classAtt)
            let expectedValue = toAttributeValue testRow index
            let predictedValue = nn.Classify testRow  
            stat.CollectStatistics(predictedValue, expectedValue)

        stat.BuildValidationStatistics()

    member nn.ValidateFromArff(testSetPath : string) : ValidationStatistics =
        let table = TableUtilities.buildTableFromArff testSetPath 
        nn.Validate(table)

    // Classifica in base all'attributo specificato in fase di training
    // La DataRow in ingresso ovviamente non deve contenere l'attributo da classificare
    // Astratto perché solo le reti concrete sanno come classificare un'istanza
    abstract member Classify : DataRow -> AttributeValue 

and TrainigFunctionType = SupervisedNeuralNetwork -> DataTable -> string -> unit      // Modifica la rete passata come primo parametro

 type NeuralLayer() =   
   inherit ResizeArray<Neuron>()  
   member this.Activate() =   
     this  
     |> Seq.iter (fun n->n.Activate() |> ignore) 

type MultiLayerNetwork(trainingFun : TrainigFunctionType) =
    inherit SupervisedNeuralNetwork(trainingFun)

    let _inputLayer = new Dictionary<string, ConstantNeuron>(HashIdentity.Structural)   // Per ogni ingresso devo sapere il neurone associato (nomi delle colonne della tabella)
    let _hiddenLayers = new ResizeArray<NeuralLayer>()                                  //TODO forse meglio una coda (in cui ci sia più il concetto di sequenza (non modificabile))
    let _outputLayer = new Dictionary<string, Neuron>(HashIdentity.Structural)          // Per ogni valore di un attributo nominal devo sapere il neurone associato (da dove li recupero -> ricavo la colonna dell'attributo (AttributeDataColumn))
                                                                                        // Se l'attributo da predire è numeric, la chiave è il nome dell'attributo
    member nn.HiddenLayers = _hiddenLayers
    member nn.InputLayer = _inputLayer
    member nn.OutputLayer = _outputLayer

    member nn.CreateNetork(trainingSet : DataTable, classAtt : string, ?seed:int, ?hiddenLayers:(int*ActivationFunType*OutputFunType) list, ?outputLayer:(ActivationFunType*OutputFunType)) =    // Potrebbe accettare una lista di int che dicono quanti livelli nascosti ci devono essere e con quanti neuroni ciascuno
        nn.OutputLayer.Clear()                      // Elimino la rete precedente
        nn.HiddenLayers
        |> Seq.iter(fun el -> el.Clear())
        nn.HiddenLayers.Clear()
        nn.InputLayer.Clear()

        let seed = defaultArg seed DateTime.Now.Second
        let actFunOut, outFunOut  = defaultArg outputLayer (sumOfProducts, linear)

        let attType = (trainingSet.Columns.[classAtt] :?> AttributeDataColumn).AttributeType
        let numClasses,nomList = match attType with
                                 | AttributeType.Nominal nomList -> List.length nomList, nomList
                                 | AttributeType.Numeric -> 1,[classAtt]
                                 | _ -> failwithf "Is not possible to create a neural network for an output type of '%A'" attType
        let numAttributes = trainingSet.Columns.Count

        let hiddenLayers = defaultArg hiddenLayers [(numClasses + numAttributes)/2, sumOfProducts, sigmoid] |> List.mapi (fun i el -> (i, el))
        let random = new Random(seed)

        // Costruisco i neuroni di input
        for col in trainingSet.Columns do
            if col.ColumnName <> classAtt then
                let neuron = new ConstantNeuron()
                nn.InputLayer.Add(col.ColumnName, neuron)

        // Costruisco i neuroni e i livelli nascosti
        for layerIndex, (layerCount, actFun, outFun) in hiddenLayers do
            let hidLayer = new NeuralLayer()
            let prevLayer = if layerIndex = 0 then
                                nn.InputLayer.Values |> Seq.map (fun el -> el :> Neuron) |> Seq.toList
                            else 
                                nn.HiddenLayers.[layerIndex-1] |> Seq.toList
            for i in 1..layerCount do
                let hid = new Neuron()
                hid.ActivationFunction <- actFun
                hid.OutputFunction <- outFun
                for prevNeur in prevLayer do
                    hid.InputMap.Add(prevNeur, (random.NextDouble()-0.5))
                hidLayer.Add(hid)
            nn.HiddenLayers.Add(hidLayer) 

        // Costruisco i neuroni di output
        for i in 1..numClasses do
            let out = new Neuron()
            out.ActivationFunction <- actFunOut
            out.OutputFunction <- outFunOut
            for prevNeur in nn.HiddenLayers.[nn.HiddenLayers.Count-1] do
                out.InputMap.Add(prevNeur, (random.NextDouble()-0.5))
            nn.OutputLayer.Add(nomList.[i-1], out)
        ()

    member nn.Activate(row : DataRow) = 
        // Imposto gli ingressi nei neuroni dell'inputLayer
        // Devo skippare l'attributo da classificare
        for col in nn.TrainingTable.Columns do
            if col.ColumnName <> nn.TrainedAttribute then
                let element = toAttributeValue row col.Ordinal
                let inVal = match element with
                            | Numeric(n) -> n
                            | String(_) -> 0.0
                            | Nominal(_, i) -> Convert.ToDouble(i)
                            | Missing -> 0.0 

                _inputLayer.[col.ColumnName].SetOutput(inVal)

        _hiddenLayers
        |> Seq.iter (fun layer -> layer.Activate())         // Attivo i layer in sequenza

        _outputLayer.Values
        |> Seq.iter (fun neur -> neur.Activate() |> ignore)

    override nn.Classify(row) =
        nn.Activate(row)
         
        if _outputLayer.Keys.Count = 1 then                         // L'attributo da classificare è numerico
            Numeric(_outputLayer.[nn.TrainedAttribute].Output)
        else                                                        // L'attributo da classificare è nominale
            let outputs = _outputLayer.Keys                                     // Costruisco una sequenza di tuple con nome del valore e uscita del relativo neurone
                            |> Seq.map (fun s -> (s, _outputLayer.[s].Output))
            let max = outputs                                                   // Ottengo una tupla con il nome del valore con l'uscita massima e la relativa uscita
                        |> Seq.maxBy (fun (_, value) -> value) 
            let attType = (nn.TrainingTable.Columns.[nn.TrainedAttribute] :?> AttributeDataColumn).AttributeType
            let nomList = match attType with
                            | AttributeType.Nominal(list) -> list
                            | _ -> failwith "never here!!!"
            let attVal = fst max
            let index = (List.findIndex (fun el -> el = attVal) nomList) + 1
            Nominal(attVal, index)                     
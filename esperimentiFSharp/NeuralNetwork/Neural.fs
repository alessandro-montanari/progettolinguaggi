module Neural

open System
open System.Data
open System.IO
open System.Collections.Generic
open NeuralTypes
open TableUtilities

// In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

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


//type  OutputValue =             // La SupervisedNeuralNetwork ritorna un risultato solo di uno di questi due tipi
//    | Numeric of double
//    | Nominal of string
//    
//    static member Create(value : double) = Numeric(value)
//    static member Create(value : string) = Nominal(value)
//    static member Create(value : obj) =
//        let valueType = value.GetType()
//        if valueType = typeof<double> then
//            Numeric(Convert.ToDouble(value))
//        elif valueType = typeof<string> then
//            Nominal(value.ToString())
//        else
//            failwithf "The type '%A' is not supported as OutputValue" valueType 

type ValidationStatistics() =
    
    let mutable _nExps = 0
    let mutable _nMiss = 0
    let mutable _nCorr = 0
    let mutable _missExps = new ResizeArray<DataRow * AttributeValue>()
    let mutable _corrExps = new ResizeArray<DataRow * AttributeValue>()

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
        let stat = new ValidationStatistics()
        let colNames = seq{ for col in testTableIn.Columns -> col.ColumnName }    // costruisco l'array di colonne che mi servono (non c'è quella del classAtt)
                    |> Seq.filter (fun name -> name <> _classAtt)
                    |> Array.ofSeq
        let testTable = testTableIn.DefaultView.ToTable("testTable", false, colNames)
        let expectedTable = testTableIn.DefaultView.ToTable("expectedTable", false, [| _classAtt |])
        
        for i in 0..(testTable.Rows.Count-1) do
            let expRow = expectedTable.Rows.[i]
            let testRow = testTable.Rows.[i]
            let outputValue = nn.Classify testRow
            if outputValue = (expRow.[0] :?> AttributeValue) then        // L'operatore '=' funziona anche sulle Discriminated Unions
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

    member nn.CreateNetork(trainingSet : DataTable, classAtt : string) =    // Potrebbe accettare una lista di int che dicono quanti livelli nascosti ci devono essere e con quanti neuroni ciascuno
        let attType = (trainingSet.Columns.[classAtt] :?> AttributeDataColumn).AttributeType
        let numClasses,nomList = match attType with
                                 | AttributeType.Nominal nomList -> List.length nomList, nomList
                                 | AttributeType.Numeric -> 1,[classAtt]
                                 | _ -> failwithf "Is not possible to create a neural network for an output type of '%A'" attType
        let numAttributes = trainingSet.Columns.Count

//        let NN  = new MultiLayerNetwork(TrainigAlgorithmBuilder.backPropagation)
        let hiddenLayersNeurons = [(numClasses + numAttributes)/2] |> List.mapi (fun i el -> (i, el))
        let random = new Random()
        let actFunHid = sumOfProducts
        let outFunHid = sigmoid
        let actFunOut = sumOfProducts
        let outFunOut = sigmoid

        // Costruisco i neuroni di input
        for col in trainingSet.Columns do
            if col.ColumnName <> classAtt then
                let neuron = new ConstantNeuron()
                nn.InputLayer.Add(col.ColumnName, neuron)

        // Costruisco i neuroni e i livelli nascosti
        for layerIndex, layerCount in hiddenLayersNeurons do
            let hidLayer = new NeuralLayer()
            let prevLayer : Neuron list =   if layerIndex = 0 then
                                                    nn.InputLayer.Values |> Seq.map (fun el -> el :> Neuron) |> Seq.toList
                                                else 
                                                    nn.HiddenLayers.[layerIndex] |> Seq.toList
            for i in 1..layerCount do
                let hid = new Neuron()
                hid.ActivationFunction <- actFunHid
                hid.OutputFunction <- outFunHid
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
                let element = row.[col.ColumnName] :?> AttributeValue
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


//
//open System.Windows.Forms
//    
//type MyType =
//    | Num of double
//    | String of string * int
//
//    override this.ToString() =
//        match this with
//        | Num(n) -> Convert.ToString(n)
//        | String(s,_) -> s
//
//let num = String("ciao", 7)
//let list = [| Num(6.8); String("ciao", 5)|]
//
//let table = new DataTable()
//table.Columns.Add("col1", num.GetType())
//let row = table.NewRow()
//row.[0] <- num
//table.Rows.Add(row)
//
//let form = new Form()
//let grid = new DataGridView(DataSource=table, Dock=DockStyle.Fill)
//form.Controls.Add(grid)
//form.Visible <- true
//
//let num2 = table.Rows.[0].[0] :?> MyType
//match num2 with
//| String(s,i) -> printfn "%s - %d" s i
module Neural

open System
open System.Data
open System.IO
open System.Collections.Generic
open NeuralTypes

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

    member nn.TrainedAttribute = _classAtt
    member val TrainingFunction = trainingFun with get, set
    
    member nn.Train(trainingSet : DataTable, classAtt : string) =
        _classAtt <- classAtt
        trainingFun nn trainingSet classAtt

    member nn.TrainFromArff(trainingSetPath : string, classAtt : string) = 
        let table = TableUtilities.buildTableFromArff trainingSetPath      
        nn.Train(table, classAtt)

    member nn.Validate(testTable : DataTable) : ValidationStatistics =       
        let stat = new ValidationStatistics()
        let colNames = seq{ for col in testTable.Columns -> col.ColumnName }    // costruisco l'array di colonne che mi servono (non c'è quella del classAtt)
                    |> Seq.filter (fun name -> name <> _classAtt)
                    |> Array.ofSeq
        let testTable = testTable.DefaultView.ToTable("testTable", false, colNames)
        let expectedTable = testTable.DefaultView.ToTable("expectedTable", false, [| _classAtt |]) 
        for testRow in testTable.Select() do
            for expRow in expectedTable.Select() do
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

    member nn.CreateNetworkAndTrain(trainingSet : DataTable, classAtt : string) =
        nn.CreateNetork(trainingSet, classAtt)
        nn.Train(trainingSet, classAtt)

    member nn.CreateNetork(trainingSet : DataTable, classAtt : string) =    // Potrebbe accettare una lista di int che dicono quanti livelli nascosti ci devono essere e con quanti neuroni ciascuno
        ()

    override nn.Classify(row) =
        // Imposto gli ingressi nei neuroni dell'inputLayer
        for col in row.Table.Columns do
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
        
        if _outputLayer.Keys.Count = 1 then                         // L'attributo da classificare è numerico
            Numeric(_outputLayer.[nn.TrainedAttribute].Output)
        else                                                        // L'attributo da classificare è nominale
            let outputs = _outputLayer.Keys                                     // Costruisco una sequenza di tuple con nome del valore e uscita del relativo neurone
                            |> Seq.map (fun s -> (s, _outputLayer.[s].Output))
            let max = outputs                                                   // Ottengo una tupla con il nome del valore con l'uscita massima e la relativa uscita
                        |> Seq.maxBy (fun (_, value) -> value) 
            Nominal(match max with |(str, _) -> (str,-1) )                      //TODO il secondo parametro non può essere -1, altrimenti non funziona l'uguale in Validation()

//TODO Come fa la funzione di train che è esterna ad accedere alle cose interne alla MultiLayerNetwork -> proprietà pubbliche


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
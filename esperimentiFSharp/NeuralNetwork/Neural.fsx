open System.Data
open System.IO
open System.Text.RegularExpressions

// In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

// - Applicare la memoizzazione sulle funzioni come Mean, ...
// - Usare un DataSet per rappresentare le tabelle (forse conviene)
// - Usare le sequence (con using) per leggere il file

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
                             

//NOTE
// - così non riesco a specificare le connessioni tra i singoli neuroni (solo livelli completamente connessi)
// - come rappresento la rete da passare agli algoritmi di training??
// Le due cose sono legate -> c'è poca struttura

open System.Collections.Generic

// Con questa modellazione di neurone posso creare qualsiasi struttura
// Comunque mi limito a supportare principlamente addestramento di tipo supervisionato in cui si ha sempre un trainig set d'ingresso
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
    
    member val TrainingFunction = trainingFun with get, set
    
    member nn.Train(trainingSet : DataTable, classAtt : string) =
        trainingFun nn trainingSet classAtt

    member nn.Train(trainingSetPath : string, classAtt : string) =  // costruisce il data table e chiama l'altro metodo
        let table = FileUtilities.LoadFile trainingSetPath      
        nn.Train(table, classAtt)

    member nn.Validate(testSet : DataTable) : ValidationStatistics =       // Li posso già implementare invocando Classify
        new ValidationStatistics()

    member nn.Validate(testSetPath : string) : ValidationStatistics =
        new ValidationStatistics()

    // Classifica in base all'attributo specificato in fase di training
    // Ritorna Numeric o Nominal in base all'attributo scelto per il training
    abstract member Classify : DataRow -> OutputValue

and TrainigFunctionType = SupervisedNeuralNetwork -> DataTable -> string -> unit      // Modifica la rete passata come primo parametro


    


// Come rappresento le istanze??    -> DataRow (fornire all'esterno il metodo DataTable.NewRow())
// Come rappresento un dataset??    -> DataTable

// vedere i metodi DataTable.Select() e Compute() e adeguare la grammatica

let table = new DataTable("MyTable")
table.Columns.Add("col a1")
table.Columns.Add("col2")
let data = seq{ for i in 0 .. 1000000 -> (i, i*i) } 
            |> Seq.map (fun (e1, e2) -> (box e1, box e2)) 
            |> Seq.map (fun (e1, e2) -> [| e1; e2 |] ) 
            |> Seq.iter (fun arr -> table.LoadDataRow(arr, false) |> ignore)

open System.Windows.Forms
let grid = new DataGrid(Dock=DockStyle.Fill, DataSource=table)
grid.ReadOnly <- true
let form = new Form(Visible=true)
form.Controls.Add(grid)

//string CSVFilePathName = @"C:\test.csv";
//string[] Lines = File.ReadAllLines(CSVFilePathName);
//string[] Fields;
//Fields = Lines[0].Split(new char[] { ',' });
//int Cols = Fields.GetLength(0);
//DataTable dt = new DataTable();
////1st row must be column names; force lower case to ensure matching later on.
//for (int i = 0; i < Cols; i++)
//    dt.Columns.Add(Fields[i].ToLower(), typeof(string));
//DataRow Row;
//for (int i = 1; i < Lines.GetLength(0); i++)
//{
//    Fields = Lines[i].Split(new char[] { ',' });
//    Row = dt.NewRow();
//    for (int f = 0; f < Cols; f++)
//        Row[f] = Fields[f];
//    dt.Rows.Add(Row);
//}


   

let layer1 = [ for i in 0 .. 9 -> new Neuron(3, sumOfProducts, sigmoid) ]
let layer2 = [ for i in 0 .. 4 -> new Neuron(10, sumOfProducts, sigmoid) ]
let layer3 = [ new Neuron(5, sumOfProducts, sigmoid) ]
let inputs = [ 0.5; 0.000099; 0.06 ];;

let outputsLayer1 = layer1 
                        |> List.map (fun n -> n.Activate(inputs))

let outputsLayer2 = layer2
                        |> List.map (fun n -> n.Activate(outputsLayer1))

let outputLayer3 = layer3
                    |> List.map (fun n -> n.Activate(outputsLayer2))

    




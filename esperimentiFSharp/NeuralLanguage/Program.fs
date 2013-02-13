// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open AST
open NeuralLanguageParser
open NeuralLanguageLex
open System.Windows.Forms
open System.Drawing
open System
open System.Collections.Generic
open Parameter
open Neural
open System.Data
open Builder
open TableUtilities

//[<EntryPoint>]
//let main argv = 
//    let lexbuf = Lexing.LexBuffer<_>.FromString "    \n ciao:-3.5 , () {} [] ATT3 ATT0 ATT_3 att3 AtT4 < >> >= && || PREPROCESSING sin + - ^ 4 \"ciao ciao\" IST0"
//    let rec loop token =
//        printfn "%A" token
//        if token <> NeuralLanguageParser.EOF then
//            loop (NeuralLanguageLex.tokenize lexbuf)
//    loop (NeuralLanguageLex.tokenize lexbuf)
//    System.Console.ReadLine() |> ignore
//    0 


// Note:
//[<EntryPoint>]
//let main argv = 
//    let lexbuf = Lexing.LexBuffer<_>.FromString "p17 : mean(ciao), \n\
//                                                 p11 : 4.5 + -5 \n\
//                                                 PREPROCESSING \n\
//                                                 { TRAINING_SET : \"C:\ciao\ciao\ciao.arff\" \n\
//                                                    ATTRIBUTE \n\
//                                                    { } \n\
//                                                    INSTANCE \n\
//                                                    { } \n\
//                                                  } \n\
//                                                  \n\
//                                                  MultiLayerNetwork \n\
//                                                  { \n\
//                                                    p17 : mean(ciao) \n\
//                                                    INPUT_LAYER \n\
//	                                                { \n\
//                                                        p17 : mean(ciao), \n\
//                                                        p11 : 4.5 + -5 \n\
//                                                    } \n\
//                                                    \n\
//                                                    HIDDEN_LAYER \n\
//                                                    { \n\
//                                                        p17 : mean(ciao), \n\
//                                                        p11 : 4.5 + -5 \n\
//                                                    } \n\
//                                                  } \n\
//                                                  \n\
//                                                  TRAINING BackPropagation \n\
//                                                  { \n\
//                                                    p17 : mean(ciao), \n\
//                                                    p11 : 4.5 + -5, \n\
//                                                    p17 : mean(ciao), \n\
//                                                    p11 : 4.5 + -5 \n\
//                                                  } \n\
//                                                  VALIDATION \n\
//                                                  { \n\
//                                                    TEST_SET : \"C:\\test.arrf\" \n\
//                                                    p11 : 4.5 + -5, \n\
//                                                    p17 : mean(ciao), \n\
//                                                    p11 : 4.5 + -5 \n\
//                                                  }" 
//    try
//        printfn "%A" (NeuralLanguageParser.start NeuralLanguageLex.tokenize lexbuf)
//    with e ->
//        let pos = lexbuf.EndPos 
//        failwithf "Error near line %d, character %d\n" pos.Line pos.Column
//    System.Console.ReadLine() |> ignore
//    0


let sampleCode = "p17 : mean(ciao), \n\
                                                 p11 : 4.5 + -5 \n\
                                                 PREPROCESSING \n\
                                                 { 
                                                 \t TRAINING_SET : \"C:\ciao\ciao\ciao.arff\" \n\
                                                    \t ATTRIBUTE \n\
                                                    { } \n\
                                                    INSTANCE \n\
                                                    { } \n\
                                                  } \n\
                                                  \n\
                                                  MultiLayerNetwork \n\
                                                  { \n\
                                                    p17 : mean(ciao) \n\
                                                    INPUT_LAYER \n\
	                                                { \n\
                                                        p17 : mean(ciao), \n\
                                                        p11 : 4.5 + -5 \n\
                                                    } \n\
                                                    \n\
                                                    HIDDEN_LAYER \n\
                                                    { \n\
                                                        p17 : mean(ciao), \n\
                                                        p11 : 4.5 + -5 \n\
                                                    } \n\
                                                  } \n\
                                                  \n\
                                                  TRAINING BackPropagation \n\
                                                  { \n\
                                                    p17 : mean(ciao), \n\
                                                    p11 : 4.5 + -5, \n\
                                                    p17 : mean(ciao), \n\
                                                    p11 : 4.5 + -5 \n\
                                                  } \n\
                                                  VALIDATION \n\
                                                  { \n\
                                                    TEST_SET : \"C:\\test.arrf\" \n\
                                                    p11 : 4.5 + -5, \n\
                                                    p17 : mean(ciao), \n\
                                                    p11 : 4.5 + -5 \n\
                                                  }" 

let code = "LOAD_TRAINING : \"C:\ciao\ciao\ain.dll\",
LOAD_NETWORK : \"C:\ciao\ciao\et.dll\"

// Mettendoli qui non perdo in generalità perché comunque il mio obbiettivo è quello di modellare reti con apprendimento supervisionato
// Per efficienza si può anche mettere un parametro \"TRAINING_TABLE\" che rappresenta il training_set già caricato
TRAINING_SET : \"C:\ciao\ciao\ciao.arff\"
CLASS_ATTRIBUTE : \"Type\" 

// Opzionale
PREPROCESSING 
{ 
// Nelle espressioni specificate nei filtri si può fare riferimento agli attributi della tabella del training set o con il loro nome 
//(RI, Na, \"mio att\") oppure tramite indice con il prefisso ATT<index>
// L'interpretazione dell'attributo dipende da dove compare, se compare come argomento di una aggregate function viene interpretato come 
// set di valori relativi a quell'attributo, altrimenti viene interpretato come valore singolo relativo ad una specifica riga
	// Opzionale
	ATTRIBUTE 
	{ 	
		p17(ciao: 56.0, attributes: 5.0),
		p11(ciao:4.5 +5.0, ciao: 56.0) 
	}
 
	// Opzionale
	INSTANCE 
	{ 
		p17(ciao: 56.0),
		p11(ciao:4.5 +5.0) 
	} 
} 

// Obbligatorio
NETWORK MultiLayer
{ 
// Nelle funzioni di Act si fa riferimento agli ingressi e ai pesi rispettivamente con IN<index> e WE<index>. E' possibile utilizzare IN 
// e WE per rappresentare tutti i valori di ingresso e tutti i pesi (utile per sumOfProducts)
// Nella funzione di Out si fa riferimento all'unico ingresso con IN

	SEED : 5.0					// Opzionale

	ASPECT HIDDEN_LAYER				// Opzionale
	{	 
		NEURONS : 10.0,			
		// Si potrebbe anche evitare di scrivere il nome della funzione -> si fa riferimento all'input con 'input'
		// In questo modo si deve fornire una espressione F#
		ACTIVATION_FUNCTION : 5.0,		
		OUTPUT_FUNCTION : 6.0	
	} 

	ASPECT HIDDEN_LAYER
	{	 
		NEURONS : 5.0,
		ACTIVATION_FUNCTION : \"sumOfProducts\",
		OUTPUT_FUNCTION : \"sigmoid\"
	} 

	ASPECT HIDDEN_LAYER
	{	 
		NEURONS : 2.0,
		ACTIVATION_FUNCTION : \"sumOfProducts\",
		OUTPUT_FUNCTION : \"sigmoid\"
	} 

	ASPECT OUTPUT_LAYER 				
	{	 
		ACTIVATION_FUNCTION : \"sumOfProducts\",	
		OUTPUT_FUNCTION : \"linear\"			
	} 
} 

// Obbligatorio
TRAINING BackPropagation 
{ 
	LEARNING_RATE : 0.3,
	EPOCHS : 500.0
} 

// Opzionale
VALIDATION 
{ 
	TEST_SET : \"C:\test.arrf\" 
}"

//// ==================================================================================
//// ========================INTERPRETE================================================
//// ==================================================================================
//
//// Mi serve un'Environment "doppio" per tenere i valori scalari e le liste di valori
//type Environment() =
//    
//    let _envSingle = new Dictionary<string, double>(HashIdentity.Structural)
//    let _envSeries = new Dictionary<string, double list>(HashIdentity.Structural)
//
//    member this.EnvSingle = _envSingle
//    member this.EnvSeries = _envSeries
//
///// Evaluate a value
//let rec evalValue factor (env:Environment) =
//    match factor with
//    | Double x   -> x
//    | Id id -> env.EnvSingle.[id]
//    | Boolean b -> match b with
//                    | true -> 1.0
//                    | false -> 0.0
//    | Function (name,ex) -> evalFunction name (evalExpression ex env)
//    | AggregateFunction (name, exList) -> evalAggregateFunction name (exList |> List.collect(fun exp -> match exp with
//                                                                                                        | Value(Id(name)) when env.EnvSeries.ContainsKey(name) -> env.EnvSeries.[name]   // Se mi trovo un ID in una aggregate function, lo vado a cercare in un altro env
//                                                                                                        | exp -> [evalExpression exp env]) )
//
//    | SumOfProducts(exList1, exList2) -> List.fold2 (fun acc el1 el2 -> acc + (evalExpression el1 env)*(evalExpression el2 env)) 0.0 exList1 exList2
//
//and evalAggregateFunction name (paramList: double list) =
//    match name with
//    | "min" -> paramList |> List.fold (fun prevMin currVal -> Math.Min(prevMin, currVal)) Double.MaxValue
//    | "max" -> paramList |> List.fold (fun prevMin currVal -> Math.Max(prevMin, currVal)) Double.MinValue
//    | "sum" -> paramList |> List.sum
//    | "sumsquared" -> paramList |> List.map(fun el -> el*el) |> List.sum
//    | "mean" -> paramList |> List.average
//    | "sd" ->   let avg = paramList |> List.average
//                sqrt (List.fold (fun acc elem -> acc + (float elem - avg) ** 2.0 ) 0.0 paramList / float paramList.Length)     
//
//and evalFunction name param =
//    match name with
//    | "sin" -> Math.Sin(param)
//    | "cos" -> Math.Cos(param)
//    | "acos" -> Math.Acos(param)
//    | "asin" -> Math.Asin(param)
//    | "tan" -> Math.Tan(param)
//    | "atan" -> Math.Atan(param)
//    | "sinh" -> Math.Sinh(param)
//    | "tanh" -> Math.Tanh(param)
//    | "log" -> Math.Log10(param)
//    | "ln" -> Math.Log(param)
//    | "floor" -> Math.Floor(param)
//    | "ceil" -> Math.Ceiling(param)
//    | "sqrt" -> Math.Sqrt(param)
//    | "exp" -> Math.Exp(param)
//    | "abs" -> Math.Abs(param)
//
///// Evaluate an expression
//and evalExpression expr env =
//    let op f v v' = 
//        if (f v v') then
//            1.0
//        else
//            0.0
//    match expr with
//    | Plus (ex1, ex2)  -> (evalExpression ex1 env) + (evalExpression ex2 env)
//    | Minus (ex1, ex2) -> (evalExpression ex1 env) - (evalExpression ex2 env)
//    | Times (ex1, ex2)  -> (evalExpression ex1 env) * (evalExpression ex2 env)
//    | Pow (ex1, ex2) -> (evalExpression ex1 env) ** (evalExpression ex2 env)
//    | Divide (ex1, ex2) -> (evalExpression ex1 env) / (evalExpression ex2 env)
//    | Negative ex1 -> -(evalExpression ex1 env)
//    | Value value -> evalValue value env
//    | And(ex1, ex2) -> match (evalExpression ex1 env), (evalExpression ex2 env) with
//                        | 1.0,1.0 -> 1.0
//                        | _, _ -> 0.0
//    | Or(ex1, ex2) -> match (evalExpression ex1 env), (evalExpression ex2 env) with
//                        | 1.0,_ | _, 1.0 -> 1.0
//                        | _, _ -> 0.0
//    | Not ex -> match (evalExpression ex env) with
//                | 1.0 -> 0.0
//                | _ -> 1.0
//    | Lt(ex1, ex2) -> op (<) (evalExpression ex1 env) (evalExpression ex2 env)
//    | Lte(ex1, ex2) -> op (<=) (evalExpression ex1 env) (evalExpression ex2 env)
//    | Gt(ex1, ex2) -> op (>) (evalExpression ex1 env) (evalExpression ex2 env)
//    | Gte(ex1, ex2) -> op (>=) (evalExpression ex1 env) (evalExpression ex2 env)
//    | Eq(ex1, ex2) -> op (=) (evalExpression ex1 env) (evalExpression ex2 env)
//    | NotEq(ex1, ex2) -> op (<>) (evalExpression ex1 env) (evalExpression ex2 env)
//
//
//let env = new Environment()
//let nOfInstances = 10
//let attList = new ResizeArray<string>()
//
//let evalInstanceListElement = function
//    | InstIndex idx -> if idx >= 0 && idx <= nOfInstances-1 then
//                            [idx]
//                       else
//                             failwithf "An instance with index '%d' is not defined" idx
//    | InstSequence(idx1, idx2) -> let indexList = if idx1 <= idx2 then
//                                                        [idx1..idx2]
//                                                  else
//                                                        [idx1..(-1)..idx2]
//                                  if List.forall(fun el ->  el >= 0 && el <= nOfInstances-1) indexList then
//                                        indexList
//                                  else
//                                        failwithf "The instance range specified is not valid."
//
//let evalNumberListElement = function
//    | NumberListElement.Exp exp -> [evalExpression exp env]
//    | NumberSequence(exp1, exp2) -> let val1 = evalExpression exp1 env
//                                    let val2 = evalExpression exp2 env
//                                    if val1 <= val2 then
//                                        [val1..val2]
//                                    else
//                                        [val1..(-1.0)..val2]
//
//let evalAttributeListElement = function
//    | AttName name -> if attList.Contains(name) then
//                        [name]
//                      else
//                        failwithf "An attribute with name '%s' is not defined" name
//    | AttIndex idx -> try
//                        [attList.[idx]]
//                      with
//                       | :? ArgumentOutOfRangeException as e -> failwithf "An attribute with index '%d' is not defined" idx
//    | AttSequence(idx1, idx2) -> let indexList = if idx1 <= idx2 then
//                                                    [idx1..idx2]
//                                                 else
//                                                    [idx1..(-1)..idx2]
//                                 try
//                                    indexList |> List.map(fun idx -> attList.[idx])
//                                 with
//                                 | :? ArgumentOutOfRangeException as e -> failwith "The attribute range specified is not valid. 
//                                                                                    An attribute with index '%d' is not defined" (Convert.ToInt32(e.ActualValue))
//
//let evalParameterValue value = function
//    | Exp exp -> exp |> box              // Non la valuto subito ma la setto nel parameter store, sarà chi la utilizza a valutarla, quindi NeuralNetork dipende da NeuralLanguage e vice-versa. Forse conviene trattarla come stringa
//    | String str -> str |> box      
//    | InstList (compl, list) -> let resList = list |> List.collect(fun el -> evalInstanceListElement el) |> Set.ofList |> Set.toList
//                                if compl then
//                                    resList |> box
//                                else
//                                    [0..nOfInstances] |> List.filter(fun el -> not (List.exists(fun el2 -> el = el2) resList) ) |> box
//    | NumList list  -> list |> List.collect(fun el -> evalNumberListElement el) |> box
//    | AttList (compl, list) ->  let resList = list |> List.collect(fun el -> evalAttributeListElement el) |> Set.ofList |> Set.toList
//                                if compl then
//                                    resList |> box
//                                else
//                                    attList |> Seq.filter(fun el -> not (List.exists(fun el2 -> el = el2) resList) ) |> box
//
//let evalParameter (store:ParameterStore) = function
//    | Parameter(name, value) -> let value = evalParameterValue value
//                                store.AddValue(name, value)
//
//
//let private directiveRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
//directiveRules.Add("LOAD_NETWORK", (fun name input -> if input.GetType() <> typeof<string> then
//                                                         invalidArg name "Wrong type, expected 'string'" ))
//
//directiveRules.Add("LOAD_TRAINING", (fun name input ->   if input.GetType() <> typeof<string> then
//                                                                invalidArg name "Wrong type, expected 'string'" ))
//
//let private globalRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
//globalRules.Add("TRAINING_TABLE", (fun name input ->    if not (typeof<DataTable>.IsInstanceOfType(input)) then     
//                                                            invalidArg name "Wrong type, expected 'DataTable'" ))
//
//globalRules.Add("TRAINING_SET", (fun name input -> if input.GetType() <> typeof<string> then
//                                                        invalidArg name "Wrong type, expected 'string'" ))
//
//globalRules.Add("CLASS_ATTRIBUTE", (fun name input ->   if input.GetType() <> typeof<string> then
//                                                            invalidArg name "Wrong type, expected 'string'" ))
//// TODO Try..with con messaggi di errore??
//let evalNetwork (net:Network) : (SupervisedNeuralNetwork * DataTable * ValidationStatistics option) =
//    let networkBuilderFactory = new BuilderFactory<Builder<SupervisedNeuralNetwork>, SupervisedNeuralNetwork>()
//    let trainingBuilderFactory = new BuilderFactory<Builder<TrainigFunctionType>, TrainigFunctionType>()
//    
//    let directiveStore = new ParameterStore(directiveRules)
//    let globalStore = new ParameterStore(globalRules)
//
//    // Valuto le direttive e carico i builder
//    net.Directives |> List.iter(fun par -> evalParameter directiveStore par ) 
//    directiveStore.ParameterValues |> Seq.iter(fun (name, values) -> if name = "LOAD_NETWORK" then
//                                                                        values |> Seq.iter(fun value -> networkBuilderFactory.LoadBuilder(value.ToString()) |> ignore)
//                                                                     elif name = "LOAD_TRAINING" then
//                                                                        values |> Seq.iter(fun value -> trainingBuilderFactory.LoadBuilder(value.ToString()) |> ignore) )
//
//    // Carico il training set
//    globalStore.AddValue("TRAINING_SET", net.TrainingSet)                       // Controllo se presente come file
//    globalStore.AddValue("CLASS_ATTRIBUTE", net.ClassAttribute)                 // Controllo se presente come attributo
//    let trainingSet = buildTableFromArff net.TrainingSet
//    globalStore.AddValue("TRAINING_SET", trainingSet) 
//
//
//
//
//
//    ()
//    
    



let createEditor =
    let text = new SyntaxHighlighter.SyntaxRichTextBox(Dock=DockStyle.Fill, AcceptsTab=true)
    text.Settings.Keywords.Add("INSTANCE")
    text.Settings.Keywords.Add("ATTRIBUTE")
    text.Settings.Keywords.Add("PREPROCESSING")
    text.Settings.Keywords.Add("VALIDATION")
    text.Settings.Keywords.Add("TRAINING")
    text.Settings.Keywords.Add("NETWORK")
    text.Settings.Keywords.Add("TRAINING_SET")
    text.Settings.Keywords.Add("CLASS_ATTRIBUTE")
    text.Settings.Keywords.Add("ASPECT")
    text.Settings.Keywords.Add("!")
    text.Settings.Keywords.Add("true")
    text.Settings.Keywords.Add("false")
//    text.Settings.Keywords.Add("LOAD_TRAINING")
//    text.Settings.Keywords.Add("LOAD_NETWORK")

    text.Settings.Comment <- "//"

    text.Settings.KeywordColor <- Color.Blue;
    text.Settings.CommentColor <- Color.Green;
    text.Settings.StringColor <- Color.Purple;
    text.Settings.IntegerColor <- Color.Red;

    text.CompileKeywords()
    text.ProcessAllLines()
    text

[<EntryPoint>]
let main argv = 
    let parser (code:string) =
        let lexbuf = Lexing.LexBuffer<_>.FromString code
        try
            NeuralLanguageParser.start NeuralLanguageLex.tokenize lexbuf
        with
        | exn -> failwithf "parse error near, line: %d - column: %d" lexbuf.EndPos.Line lexbuf.EndPos.Column

    let buildInterface (parser : string -> Network) =
        let form = new Form(Visible=true)
        let text = createEditor
        text.LoadFile(@"C:\Users\Alessandro\Desktop\repo-linguaggi\esperimentiFSharp\NeuralLanguage\Code.txt")
        text.Font <- new Font(FontFamily.GenericMonospace, float32(10.0))
        text.ProcessAllLines()
        let btnParse = new Button(Dock=DockStyle.Bottom, Text="Parse!")
        btnParse.Font <- new Font(btnParse.Font, btnParse.Font.Style ||| FontStyle.Bold)
        let btnSave = new Button(Dock=DockStyle.Bottom, Text="Save")
        btnSave.Click.Add(fun _ ->  text.SaveFile(@"C:\Users\Alessandro\Desktop\repo-linguaggi\esperimentiFSharp\NeuralLanguage\Code.txt"))
        let console = new RichTextBox(Dock=DockStyle.Fill)
        console.Font <- new Font(FontFamily.GenericMonospace, float32(10.0))
        btnParse.Click.Add(fun _ -> try
                                        console.Text <- sprintf "%A" (parser text.Text)
                                    with e ->
                                        console.Text <- "Error"
                                    )
        let splitContainerVer = new SplitContainer(Dock=DockStyle.Fill)
        let splitContainerHor = new SplitContainer(Dock=DockStyle.Fill, Orientation=Orientation.Horizontal)
        splitContainerVer.Panel2.Controls.Add(splitContainerHor)
        let treeView = new TreeView(Dock=DockStyle.Fill)
        treeView.BeginUpdate()
        treeView.Nodes.Add("Parent") |> ignore
        treeView.Nodes.[0].Nodes.Add("Child 1") |> ignore
        treeView.Nodes.[0].Nodes.Add("Child 2") |> ignore
        treeView.Nodes.[0].Nodes.[1].Nodes.Add("Grandchild") |> ignore
        treeView.Nodes.[0].Nodes.[1].Nodes.[0].Nodes.Add("Great Grandchild") |> ignore
        treeView.EndUpdate()
        splitContainerVer.Panel1.Controls.Add(treeView)
        splitContainerHor.Panel1.Controls.Add(text)
        splitContainerHor.Panel2.Controls.Add(console)
        form.Controls.Add(splitContainerVer)
        form.Controls.Add(btnParse)
        form.Controls.Add(btnSave)
        Application.Run(form)
        Application.EnableVisualStyles()
    buildInterface parser
    0
    
    
 

// let bigTest = "p0 : -0.7 ; \n\
//                p1 : ATT4+4.0; \n\
//                p2 : myatt*-4.9; \n\
//                p3 : 5.0 + 6.0 +7.0; \n\
//                p4 : 5.0 - 6.0 - 7.0; \n\
//                p5 : 5.0 > 6.0 > 7.0; \n\
//                p6 : (3.0+5.0)*(9.0 -2.0); \n\
//                p7 : [ciao, ciao, ciao]; \n\
//                p8 : [ciao, ATT0, ATT6 .. ATT10]; \n\
//                p9 : [4.0, 5.9, 5.6 .. 90.99]; \n\
//                p10 : true && false + sin(4.0); \n\
//                p11 : 4.5 + -5; \n\
//                p12 : [ATT3, \"mio _ * - ^ att\", ATT9..ATT20]; \n\
//                p13 : 5^3^2; \n\
//                p14: !4.5 && !7; \n\
//                p15 : -(3+4*5); \n\
//                p16 : !(5); \n\
//                p17 : mean(ciao); \n\
//                p18 : mean(\"ciao ciao\"); \n\
//                p19 : mean(ATT5); \n\
//                p20 : [INST1, INST3, INST0..INST6, INST7]; \n\
//                p21 : ![ATT4, ATT6] \n\
//                PREPROCESSING \n\
//                { \n\
//                TRAINING_SET : \"C:\ciao\ciao\ciao.arff\" \n\
//                ATTRIBUTE \n\
//                { \n\
//                    filter1(par:6, par2:x+y, par3:ATT3+ATT5); \n\
//                    filter1(par:6, par2:x+y, par3:ATT3+ATT5) \n\
//                } \n\
//                \n\
//                INSTANCE \n\
//                { \n\
//                    filter1(par:6, par2:x+y, par3:ATT3+ATT5); \n\
//                    filter1() \n\
//                }\n\
//                } \n\
//                \n\
//                MultiLayerNetwork WITH_BIAS \n\
//                { \n\
//                p17 : mean(ciao) \n\
//                INPUT_LAYER in \n\
//	            { \n\
//                    p17 : mean(ciao); \n\
//                    p11 : 4.5 + -5 \n\
//                } \n\
//                \n\
//                HIDDEN_LAYER primo \n\
//                { \n\
//                    p17 : mean(ciao); \n\
//                    p11 : 4.5 + -5 \n\
//                } \n\
//                \n\
//                }"
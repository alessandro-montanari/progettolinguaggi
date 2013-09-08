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
open Preprocessing
open Validation
open NetworkIterpreter

open System.Threading
open System.ComponentModel


//===================== GESTIONE PROGRESS BAR ===================
// - BackGround worker solo per l'interprete
// - la progress bar riflette l'avanzamento dell'interprete (5 fasi)
// - dopo ogni fase si controlla se c'è una Cancellazione pendente
//  - splittare le fasi in funzioni diverse così da semplificarsi la gestione della cancellazione
       
        



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

let createInstanceListElement el =
    match el with
    | InstIndex(intVal) -> new TreeNode("InstIndex", [|new TreeNode(intVal.ToString())|])
    | InstSequence(startIndex, endIndex) -> new TreeNode("InstSequence", [|new TreeNode(startIndex.ToString()); new TreeNode(endIndex.ToString())|])

let createAttributeListElement el =
    match el with
    | AttName(name) -> new TreeNode("AttName", [|new TreeNode(name)|])
    | AttIndex(index) -> new TreeNode("AttIndex", [|new TreeNode(index.ToString())|])
    | AttSequence(startIndex, endIndex) -> new TreeNode("AttSequence", [|new TreeNode(startIndex.ToString()); new TreeNode(endIndex.ToString())|])

let createParameterValue el =
    match el with
    | AttList(negate, attList) -> new TreeNode(sprintf "AttList - %b" negate, attList |> List.map createAttributeListElement |> List.toArray)
    | InstList(negate, attList) -> new TreeNode(sprintf "InstList - %b" negate, attList |> List.map createInstanceListElement |> List.toArray)
    | String(value) -> new TreeNode(value)
    | Double(value) -> new TreeNode(value.ToString())
    | Integer(value) -> new TreeNode(value.ToString())
    | Bool(value) -> new TreeNode(value.ToString())

let createParameter el =
    match el with
    | Parameter(name, value) -> new TreeNode("Parameter - " + name, [| createParameterValue value |])

let createFilter el =
    match el with
    | Filter(name, parList) -> new TreeNode("Filter - " + name, parList |> List.map createParameter |> List.toArray )

let createAspect el =
    match el with
    | Aspect(name, parList) -> new TreeNode("Aspect - " + name, parList |> List.map createParameter |> List.toArray )

let createNetwork (net:Network) =
    let treeView = new TreeView(Dock=DockStyle.Fill)
    treeView.BeginUpdate()
    treeView.Nodes.Add("Neural Network") |> ignore

    treeView.Nodes.[0].Nodes.Add( new TreeNode("Directives", net.Directives |> List.map createParameter |> List.toArray) ) |> ignore
    treeView.Nodes.[0].Nodes.Add("TrainingSet").Nodes.Add(net.TrainingSet) |> ignore
    treeView.Nodes.[0].Nodes.Add("ClassAttribute").Nodes.Add(net.ClassAttribute) |> ignore
    let attPre, instPre = net.Preprocessing
    treeView.Nodes.[0].Nodes.Add("Preprocessing").Nodes.AddRange([| new TreeNode("Attribute", attPre |> List.map createFilter |> List.toArray);    
                                                                    new TreeNode("Instance", instPre |> List.map createFilter |> List.toArray)|])
    let netName, paramList, aspList = net.NetworkDefinition
    let array1 = paramList |> List.map createParameter |> List.toArray
    let array2 = aspList |> List.map createAspect |> List.toArray
    treeView.Nodes.[0].Nodes.Add("NetworkDefinition - " + netName).Nodes.AddRange([| array1; array2 |] |> Array.concat)

    let trainName, paramList, aspList = net.Training
    let array1 = paramList |> List.map createParameter |> List.toArray
    let array2 = aspList |> List.map createAspect |> List.toArray
    treeView.Nodes.[0].Nodes.Add("Training - " + trainName).Nodes.AddRange([| array1; array2 |] |> Array.concat)

    let paramList, aspList = net.Validation
    let array1 = paramList |> List.map createParameter |> List.toArray
    let array2 = aspList |> List.map createAspect |> List.toArray
    treeView.Nodes.[0].Nodes.Add("Validation").Nodes.AddRange([| array1; array2 |] |> Array.concat)

    treeView.EndUpdate()
    treeView



[<EntryPoint>]
[<STAThreadAttribute>]
let main argv = 
    let parser (code:string) =
        let lexbuf = Lexing.LexBuffer<_>.FromString code
        // TODO lexbuf.BufferLocalStore
        try
            NeuralLanguageParser.start NeuralLanguageLex.tokenize lexbuf
        with
        | exn -> failwithf "parse error near, line: %d - column: %d \nLast token: %s" lexbuf.StartPos.Line lexbuf.StartPos.Column (System.String(lexbuf.Lexeme))

    let buildInterface (parser : string -> Network) =
        let form = new Form(Visible=true)
        form.Size <- new Size(1200,800)
        let progressBar = new ProgressBar(Dock=DockStyle.Bottom)
        let text = createEditor
        text.Font <- new Font(FontFamily.GenericMonospace, float32(10.0))

        let console = new RichTextBox(Dock=DockStyle.Fill)
        console.Font <- new Font(FontFamily.GenericMonospace, float32(10.0))

        let splitContainerVer = new SplitContainer(Dock=DockStyle.Fill)
        splitContainerVer.SplitterDistance <- 30
        let splitContainerHor = new SplitContainer(Dock=DockStyle.Fill, Orientation=Orientation.Horizontal)
        splitContainerHor.SplitterDistance <- 70

        let toolBar = new ToolBar(Dock=DockStyle.Top)
        toolBar.Appearance <- System.Windows.Forms.ToolBarAppearance.Flat;
        let btnOpen = new ToolBarButton("Open")
        let btnSave = new ToolBarButton("Save", Enabled=false)
        let btnSaveAs = new ToolBarButton("Save As", Enabled=false)
        let sep = new ToolBarButton()
        sep.Style <- ToolBarButtonStyle.Separator
        let btnParse = new ToolBarButton("Parse and Train", Enabled=false)
        let btnCancel = new ToolBarButton("Cancel", Enabled=false)
        let btnShow = new ToolBarButton("Show Preprocessed Table", Enabled=false)
        let btnShowNet = new ToolBarButton("Show Neural Network", Enabled=false)

        toolBar.Buttons.Add(btnOpen) |> ignore
        toolBar.Buttons.Add(btnSave) |> ignore
        toolBar.Buttons.Add(btnSaveAs) |> ignore
        toolBar.Buttons.Add(sep) |> ignore
        toolBar.Buttons.Add(btnParse) |> ignore
        toolBar.Buttons.Add(btnCancel) |> ignore
        toolBar.Buttons.Add(btnShowNet) |> ignore
        toolBar.Buttons.Add(btnShow) |> ignore


        let dataSetTable = ref null
        let networkUIControl = ref null

        let worker = getEvalNetworkWorker
        worker.ProgressChanged.Add(fun arg -> progressBar.Value <- arg.ProgressPercentage)
        worker.RunWorkerCompleted.Add(fun args ->
                                         btnCancel.Enabled <- false
                                         btnParse.Enabled <- true
                                         btnShow.Enabled <- true
                                         btnShowNet.Enabled <- true
                                         if args.Cancelled then
                                            console.AppendText("Execution Cancelled.\n\n\n") 
                                            progressBar.Value <- 0                                 
                                         elif args.Error <> null then
                                            console.AppendText(sprintf "Execution aborted.\n%s\n\n\n" args.Error.Message)
                                            progressBar.Value <- 0  
                                         else
                                             let NN, table, stat, visualizer = args.Result :?> (SupervisedNeuralNetwork * DataTable * ValidationStatistics option * (SupervisedNeuralNetwork -> Control))
                                             console.AppendText("Execution terminated with success!!!\n")
                                             match stat with
                                             | Some(statVal) -> console.AppendText("Validation Statistics available:\n")
                                                                statVal.Statistics |> Seq.iter (fun el -> console.AppendText(sprintf "%s : %A\n" el.Key el.Value))
                                             | _ -> ()

                                             console.AppendText("\n\n")

                                             dataSetTable := table
                                             networkUIControl := visualizer NN

                                         console.SelectionStart <- console.Text.Length
                                         console.ScrollToCaret()
                                       )

        toolBar.ButtonClick.Add(fun args -> 
                                            if args.Button = btnCancel then
                                                worker.CancelAsync()

                                            elif args.Button = btnParse then
                                                console.AppendText("Execution started...\n")
                                                btnCancel.Enabled <- true
                                                btnParse.Enabled <- false
                                                btnShow.Enabled <- false
                                                btnShowNet.Enabled <- false
                                                try
                                                    let net = parser text.Text
                                                    let treeView = createNetwork net
                                                    splitContainerVer.Panel1.Controls.Clear()
                                                    splitContainerVer.Panel1.Controls.Add(treeView)
                                                    progressBar.Value <- 0                                
                                                    worker.RunWorkerAsync(net) 
                                                with
                                                | e -> console.AppendText(e.Message + "\n\n")
                                                       console.SelectionStart <- console.Text.Length
                                                       console.ScrollToCaret()
                                                       btnCancel.Enabled <- false
                                                       btnParse.Enabled <- true

                                                elif args.Button = btnSave then
                                                    text.SaveFile(@"C:\Users\Alessandro\Desktop\repo-linguaggi\esperimentiFSharp\NeuralLanguage\Code.txt")                                                  

                                                elif args.Button = btnShow then
                                                    let form = new Form(Visible=true)
                                                    form.Size <- new Size(700,700)
                                                    let grid = new DataGridView(DataSource=dataSetTable.Value, Dock=DockStyle.Fill)
                                                    form.Controls.Add(grid)

                                                elif args.Button = btnShowNet then
                                                    let form = new Form(Visible=true)
                                                    form.Size <- new Size(700,700)
                                                    form.Controls.Add(networkUIControl.Value)

                                                elif args.Button = btnOpen then
                                                    let dialog = new OpenFileDialog()
                                                    dialog.InitialDirectory <- "C:";
                                                    dialog.Title <- "Select a N# file";
                                                    dialog.Filter <- "N# files (*.n#)|*.n#|All files (*.*)|*.*";
                                                     if dialog.ShowDialog() = DialogResult.OK then
                                                        text.Clear()
                                                        text.ProcessAllLines()
                                                        text.LoadFile(dialog.FileName, RichTextBoxStreamType.RichText)
                                                        text.Tag <- dialog.FileName
                                                        text.ProcessAllLines()
                                                        btnSave.Enabled <- true
                                                        btnSaveAs.Enabled <- true
                                                        btnParse.Enabled <- true

                                                elif args.Button = btnSave then
                                                    text.SaveFile(text.Tag.ToString())

                                                elif args.Button = btnSaveAs then
                                                    let dialog = new SaveFileDialog()
                                                    dialog.InitialDirectory <- "C:"
                                                    dialog.Title <- "Save a N# file"
                                                    dialog.Filter <- "N# file|*.n#"
                                                    dialog.ShowDialog() |> ignore

                                                    // If the file name is not an empty string open it for saving.
                                                    if dialog.FileName <> "" then
                                                          // Saves the file via a FileStream created by the OpenFile method.
                                                          let fs = dialog.OpenFile() :?> System.IO.FileStream;
                                                          text.SaveFile(fs, RichTextBoxStreamType.RichText)
                                                          text.Tag <- dialog.FileName
                                                          fs.Close();                                               
                                )
        
        splitContainerVer.Panel2.Controls.Add(splitContainerHor)
        splitContainerHor.Panel1.Controls.Add(text)
        splitContainerHor.Panel2.Controls.Add(console)

        form.Controls.Add(splitContainerVer)
        form.Controls.Add(toolBar)
        form.Controls.Add(progressBar)
        Application.EnableVisualStyles()
        Application.Run(form)
    buildInterface parser
    0



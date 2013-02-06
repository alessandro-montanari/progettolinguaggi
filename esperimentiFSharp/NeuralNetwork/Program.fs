module Program

open System
open System.Data
open System.Windows.Forms
open Parameter
open Validation
open Neural
open AttributePreprocessing
open InstancePreprocessing
open NeuralTypes
open MultiLayerNetwork
open System.Collections.Generic

open System 
open System.CodeDom.Compiler 
open Microsoft.FSharp.Compiler.CodeDom

//[<EntryPoint>]
//let main argv = 
//    let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\vote.arff"
//    // TEST - TableUtilities.buildTableFromArff
////    let form = new Form()
////    form.Text <- table.TableName
////    let grid = new DataGridView(DataSource=table, Dock=DockStyle.Fill)
////    form.Controls.Add(grid)
////    form.Visible <- true
////    Application.Run(form)
//
//    let valBuilder = new BasicValidationBuilder()
//    let testTable = valBuilder.BuildTestTable(table)
//    printfn "%A" (table=testTable)                      // Non sono stati settati parametri quindi ls testTable deve essere uguale alla training table (controllo tra riferimenti)
//
//    valBuilder.ParameterStore.SetValue("TEST_SET", String(@"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\weather.arff"))
//    let testTable2 = valBuilder.BuildTestTable(table)
////    let form = new Form()
////    let grid = new DataGridView(DataSource=testTable2, Dock=DockStyle.Fill)
////    form.Text <- testTable2.TableName
////    form.Controls.Add(grid)
////    form.Visible <- true
////    Application.Run(form)
//
//    valBuilder.ParameterStore.ClearParameters()
//    valBuilder.ParameterStore.SetValue("PERCENTAGE_SPLIT", Number(1.0))
//    let testTableSplit = valBuilder.BuildTestTable(table)
////    let form = new Form()
////    let grid = new DataGridView(DataSource=testTableSplit, Dock=DockStyle.Fill)
////    form.Text <- testTableSplit.TableName
////    form.Controls.Add(grid)
////    form.Visible <- true
////    Application.Run(form)
//
////    valBuilder.ParameterStore.SetValue("TEST_SET", String(@"C:"))        // Settati due parametri -> eccezione
////    let testTable3 = valBuilder.BuildTestTable(table)
//
//    System.Console.ReadLine() |> ignore
//    0


[<EntryPoint>]
let main argv = 

    let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\vote.arff"
    let classAtt = "Class"

    let globalRules = new Dictionary<string, (string -> obj -> unit)>(HashIdentity.Structural)
    globalRules.Add("TRAINING_TABLE", (fun name input ->    if not (typeof<DataTable>.IsInstanceOfType(input)) then
                                                                invalidArg name "Wrong type, expected 'DataTable'" ))

    globalRules.Add("CLASS_ATTRIBUTE", (fun name input ->   if input.GetType() <> typeof<string> then
                                                                invalidArg name "Wrong type, expected 'string'" ))
    let globalParam = new ParameterStore(globalRules)
    globalParam.AddValue("TRAINING_TABLE", table)
    globalParam.AddValue("CLASS_ATTRIBUTE", classAtt)

    let algBuilder = new BackPropagation.BackPropagationBuilder()
    algBuilder.LocalParameters.AddValue("EPOCHS",50)
    algBuilder.LocalParameters.AddValue("LEARNING_RATE",0.3)

    let networkbuilder = new MultiLayerNetworkBuilder()
    let validationBuilder = new BasicValidationBuilder()

    validationBuilder.LocalParameters.AddValue("PERCENTAGE_SPLIT", 100.0)

//    networkbuilder.AddAspect("HIDDEN_LAYER", [("NEURONS", box 20);("ACTIVATION_FUNCTION", box sumOfProducts);("OUTPUT_FUNCTION",box sigmoid)])
//    networkbuilder.AddAspect("HIDDEN_LAYER", [("NEURONS", box 40);("ACTIVATION_FUNCTION", box sumOfProducts);("OUTPUT_FUNCTION",box sigmoid)])
//    networkbuilder.AddAspect("HIDDEN_LAYER", [("NEURONS", box 10);("ACTIVATION_FUNCTION", box sumOfProducts);("OUTPUT_FUNCTION", box sigmoid)])
//    networkbuilder.AddAspect("HIDDEN_LAYER", [("NEURONS", box 5);("ACTIVATION_FUNCTION", box sumOfProducts);("OUTPUT_FUNCTION", box sigmoid)])
//    networkbuilder.AddAspect("OUTPUT_LAYER", [("ACTIVATION_FUNCTION", box sumOfProducts); ("OUTPUT_FUNCTION", box sigmoid)])
    let NN  = networkbuilder.Build(globalParam)
    NN.TrainingFunction <- algBuilder.Build(globalParam)
    NN.Train(table, classAtt)
    let stat = NN.Validate(validationBuilder.Build(globalParam))
    stat.PrintStatistcs()

    let graph = Graph.createGraphFromNetwork (NN :?> MultiLayerNetwork)
    let form = new Form()
    form.Controls.Add(graph)
    form.Visible <- true
    Application.Run(form)
   
   

//    addExpression "newAtt" "RI+100+sum(Na)" table
//    printfn "addExpression FINISHED"
//    mathExpression [("Fe", "Fe+1000"); ("Ba", "Fe-1000")] table
//    printfn "mathExpression FINISHED"

//    normalize 1.0 0.0 table
//    printfn "normalize FINISHED"

//    standardize table
//    printfn "standardize FINISHED"
//
//    removeByName ["Fe";"Ba";"newAtt";"Na"] table
//    printfn "remove FINISHED"
//
//    replaceMissingValues table
//    printfn "ReplaceMissingValues FINISHED"

//    nominalToBinary ["Type"] table
//    printfn "nominalToBinary FINISHED"

//    discretize ["RI"] 10 table
//    printfn "discretize FINISHED"

//    removeRange [0;1;2;3;4] table
//    removePercentage 55.6 table
//    subsetByExpression "Na>12.0" table

    // per predire valori numerici serve una funzione di uscita linear per il nodo di uscita
    // per predire valori nominal serve una funzione di uscita sigmoid
//    NN.CreateNetork(table, "Type", 5, [(20,sumOfProducts,sigmoid);(10,sumOfProducts,sigmoid);(5,sumOfProducts,sigmoid)], (sumOfProducts, linear))          // TODO forse un po' da migliorare l'interfaccia qui
//    NN.CreateNetork(table, "Class", outputLayer=(sumOfProducts, sigmoid))
//    NN.Train(table, "Class")
//
//    let valBuilder = new BasicValidationBuilder()
//    valBuilder.GlobalParameters.AddValue("TRAINING_TABLE", table)
//    valBuilder.GlobalParameters.AddValue("PERCENTAGE_SPLIT", 100.0)
//    let stat = NN.Validate(valBuilder.Build())
//    stat.PrintStatistcs()

//    let form = new Form()
////    let grid = new DataGridView(DataSource=table, Dock=DockStyle.Fill)
//    form.Controls.Add((Graph.createGraphFromNetwork NN))
//    form.Visible <- true
//    Application.Run(form)


//    let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\iris.arff"
//    let rows = table.Select("sepallength < 5 And sepalwidth > 3 and class = 'Iris-setosa'")
//    let table2 = table.Clone()
//    for row in rows do
//        table2.LoadDataRow(row.ItemArray, false)
////    table2.Columns.Add("Count", typeof<double>, "sepallength+1")
//    table2.Columns.Add("sepallength2").Expression <- "sepallength+100"
//    let form = new Form()
//    let grid = new DataGridView(DataSource=table2, Dock=DockStyle.Fill)
//    form.Controls.Add(grid)
//    form.Visible <- true
//    Application.Run(form)



    
    // Prova compilazione ed esecuzione codice F# da stringa

    // Our (very simple) code string consisting of just one function: unit -> string 
    let codeString =
        "let activationFunction input = input
                                          |> Seq.fold (fun acc el -> match el with (a,b) -> acc + a*b) 0.0"
                                          

    let CompileFSharpCode codeString =
            let codeString = "#light\nmodule Custom.Code\n"+codeString
            use provider = new FSharpCodeProvider() 
            let options = CompilerParameters()
            options.GenerateInMemory <- true
            let result = provider.CompileAssemblyFromSource( options, [|codeString|] ) 
            // If we missed anything, let compiler show us what's the problem
            if result.Errors.Count <> 0 then  
                for i = 0 to result.Errors.Count - 1 do
                    printfn "%A" (result.Errors.Item(i).ErrorText)
            result

    let result = CompileFSharpCode codeString
    if result.Errors.Count = 0 then
        let synthAssembly = result.CompiledAssembly
        let synthMethod  = synthAssembly.GetType("Custom.Code").GetMethod("activationFunction") 
        let input = [(1.0,2.0);(1.0,2.0);(1.0,2.0)] |> List.toSeq
        // Fare qualcosa di simile per la funzione di uscita
        let actFunction (theMethod:Reflection.MethodInfo) (input:seq<double*double>) =
            if theMethod.ReturnType <> typeof<double> then
                failwithf "%s : The return type of the function is not correct, it is '%A' but should be 'double'" theMethod.Name theMethod.ReturnType
            let returnType = theMethod.GetParameters().[0].ParameterType
            if returnType <> typeof<seq<double*double>> then
                 failwithf "%s : The input type of the function is not correct, it is '%A' but should be 'seq<double*double>'" theMethod.Name returnType
            Convert.ToDouble(theMethod.Invoke(null,[|input|]))

        let actFun = actFunction synthMethod
        printfn "Success: %A" (actFun input)
    else
         printfn "Compile error"



    System.Console.ReadLine() |> ignore
    0




   
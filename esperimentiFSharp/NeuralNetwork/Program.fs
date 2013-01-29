module Program

open System
open System.Data
open System.Windows.Forms
open Parameter
open ValidationBuilder
open Neural

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
//     let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\weather.arff"
//     let rows = table.Compute("Sum(temperature)","")
//     0




    let NN  = new MultiLayerNetwork((fun n t s -> () ))
    let const1 = new ConstantNeuron()
    let const2 = new ConstantNeuron()
    let const3 = new ConstantNeuron()
    let const4 = new ConstantNeuron()
    let const5 = new ConstantNeuron()
    NN.InputLayer.Add("outlook", const1)
    NN.InputLayer.Add("temperature", const2)
    NN.InputLayer.Add("humidity", const3)
    NN.InputLayer.Add("windy", const4) 
    NN.InputLayer.Add("play", const5)
    
    let hid1 = new Neuron()
    hid1.ActivationFunction <- sumOfProducts
    hid1.OutputFunction <- sigmoid
    hid1.InputMap.Add(const1, 0.005) 
    hid1.InputMap.Add(const2, 0.0005) 
    hid1.InputMap.Add(const3, 0.6666) 
    hid1.InputMap.Add(const4, 0.1116) 
    hid1.InputMap.Add(const5, 0.1000) 

    let hidLayer = new NeuralLayer()
    hidLayer.Add(hid1)
    NN.HiddenLayers.Add(hidLayer)

    let out1 = new Neuron()
    out1.ActivationFunction <- sumOfProducts
    out1.OutputFunction <- sigmoid
    out1.InputMap.Add(hid1, 0.09) 
    let out2 = new Neuron()
    out2.ActivationFunction <- sumOfProducts
    out2.OutputFunction <- sigmoid
    out2.InputMap.Add(hid1, 1.51) 
    NN.OutputLayer.Add("no", out1)
    NN.OutputLayer.Add("yes", out2)

    let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\weather.arff"
    NN.Train(table, "play")
    let out = NN.Classify(table.Rows.[0])
    printfn "---- OUT: %A" out
    System.Console.ReadLine() |> ignore
    0

   
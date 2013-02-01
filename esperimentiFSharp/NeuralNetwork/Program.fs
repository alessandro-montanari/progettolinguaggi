module Program

open System
open System.Data
open System.Windows.Forms
open Parameter
open ValidationBuilder
open Neural
open AttributePreprocessing
open InstancePreprocessing
open NeuralTypes

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

//    let NN  = new MultiLayerNetwork(TrainigAlgorithmBuilder.backPropagation)
//    let const1 = new ConstantNeuron()
//    let const2 = new ConstantNeuron()
//    let const3 = new ConstantNeuron()
//    let const4 = new ConstantNeuron()
//    let const5 = new ConstantNeuron()
//    let const6 = new ConstantNeuron()
//    let const7 = new ConstantNeuron()
//    let const8 = new ConstantNeuron()
//    let const9 = new ConstantNeuron()
//    let const10 = new ConstantNeuron()
//    let const11 = new ConstantNeuron()
//    let const12 = new ConstantNeuron()
//    let const13 = new ConstantNeuron()
//    let const14 = new ConstantNeuron()
//    let const15 = new ConstantNeuron()
//    let const16 = new ConstantNeuron()
//
//    NN.InputLayer.Add("handicapped-infants", const1)
//    NN.InputLayer.Add("water-project-cost-sharing", const2)
//    NN.InputLayer.Add("adoption-of-the-budget-resolution", const3)
//    NN.InputLayer.Add("physician-fee-freeze", const4) 
//    NN.InputLayer.Add("el-salvador-aid", const5)
//    NN.InputLayer.Add("religious-groups-in-schools", const6)
//    NN.InputLayer.Add("anti-satellite-test-ban", const7)
//    NN.InputLayer.Add("aid-to-nicaraguan-contras", const8) 
//    NN.InputLayer.Add("mx-missile", const9)
//    NN.InputLayer.Add("immigration", const10)
//    NN.InputLayer.Add("synfuels-corporation-cutback", const11)
//    NN.InputLayer.Add("education-spending", const12) 
//    NN.InputLayer.Add("superfund-right-to-sue", const13)
//    NN.InputLayer.Add("crime", const14)
//    NN.InputLayer.Add("duty-free-exports", const15)
//    NN.InputLayer.Add("export-administration-act-south-africa", const16) 
//    
//    let hid1 = new Neuron()
//    hid1.ActivationFunction <- sumOfProducts
//    hid1.OutputFunction <- sigmoid
//    hid1.InputMap.Add(const1, 0.005) 
//    hid1.InputMap.Add(const2, -0.0005) 
//    hid1.InputMap.Add(const3, 0.06666) 
//    hid1.InputMap.Add(const4, 0.01116) 
//    hid1.InputMap.Add(const5, -0.005) 
//    hid1.InputMap.Add(const6, 0.0005) 
//    hid1.InputMap.Add(const7, -0.06666) 
//    hid1.InputMap.Add(const8, -0.01116) 
//    hid1.InputMap.Add(const9, 0.005) 
//    hid1.InputMap.Add(const10, 0.0005) 
//    hid1.InputMap.Add(const11, -0.06666) 
//    hid1.InputMap.Add(const12, 0.01116) 
//    hid1.InputMap.Add(const13, -0.1005) 
//    hid1.InputMap.Add(const14, 0.0005) 
//    hid1.InputMap.Add(const15, -0.06666) 
//    hid1.InputMap.Add(const16, 0.01116) 
//
//
//    let hid2 = new Neuron()
//    hid2.ActivationFunction <- sumOfProducts
//    hid2.OutputFunction <- sigmoid
//    hid2.InputMap.Add(const1, 1.005) 
//    hid2.InputMap.Add(const2, 0.0005) 
//    hid2.InputMap.Add(const3, -0.06666) 
//    hid2.InputMap.Add(const4, -0.01116) 
//    hid2.InputMap.Add(const5, -0.005) 
//    hid2.InputMap.Add(const6, 0.0005) 
//    hid2.InputMap.Add(const7, 0.06666) 
//    hid2.InputMap.Add(const8, 0.01116) 
//    hid2.InputMap.Add(const9, 0.005) 
//    hid2.InputMap.Add(const10, 0.10005) 
//    hid2.InputMap.Add(const11, 1.06666) 
//    hid2.InputMap.Add(const12, 0.01116) 
//    hid2.InputMap.Add(const13, -0.005) 
//    hid2.InputMap.Add(const14, -0.0005) 
//    hid2.InputMap.Add(const15, 0.06666) 
//    hid2.InputMap.Add(const16, 0.01116) 
//
//    let hid3 = new Neuron()
//    hid3.ActivationFunction <- sumOfProducts
//    hid3.OutputFunction <- sigmoid
//    hid3.InputMap.Add(const1, 0.005) 
//    hid3.InputMap.Add(const2, 0.0005) 
//    hid3.InputMap.Add(const3, 0.16666) 
//    hid3.InputMap.Add(const4, 0.01116)
//    hid3.InputMap.Add(const5, 0.1005) 
//    hid3.InputMap.Add(const6, 1.0005) 
//    hid3.InputMap.Add(const7, 0.06666) 
//    hid3.InputMap.Add(const8, -0.01116) 
//    hid3.InputMap.Add(const9, 0.005) 
//    hid3.InputMap.Add(const10, -0.0005) 
//    hid3.InputMap.Add(const11, 0.06666) 
//    hid3.InputMap.Add(const12, -0.01116) 
//    hid3.InputMap.Add(const13, 0.005) 
//    hid3.InputMap.Add(const14, 0.0005) 
//    hid3.InputMap.Add(const15, 1.06666) 
//    hid3.InputMap.Add(const16, -0.101116) 
//
//    let hid4 = new Neuron()
//    hid4.ActivationFunction <- sumOfProducts
//    hid4.OutputFunction <- sigmoid
//    hid4.InputMap.Add(const1, 0.005) 
//    hid4.InputMap.Add(const2, 0.0005) 
//    hid4.InputMap.Add(const3, -0.06666) 
//    hid4.InputMap.Add(const4, 0.01116)
//    hid4.InputMap.Add(const5, 0.005) 
//    hid4.InputMap.Add(const6, 0.0005) 
//    hid4.InputMap.Add(const7, -0.06666) 
//    hid4.InputMap.Add(const8, -0.01116) 
//    hid4.InputMap.Add(const9, 0.005) 
//    hid4.InputMap.Add(const10, 0.0005) 
//    hid4.InputMap.Add(const11, 0.06666) 
//    hid4.InputMap.Add(const12, 0.01116) 
//    hid4.InputMap.Add(const13, 0.005) 
//    hid4.InputMap.Add(const14, 0.0005) 
//    hid4.InputMap.Add(const15, -0.06666) 
//    hid4.InputMap.Add(const16, 0.01116) 
//
//    let hid5 = new Neuron()
//    hid5.ActivationFunction <- sumOfProducts
//    hid5.OutputFunction <- sigmoid
//    hid5.InputMap.Add(const1, -0.005) 
//    hid5.InputMap.Add(const2, -0.0005) 
//    hid5.InputMap.Add(const3, 0.06666) 
//    hid5.InputMap.Add(const4, 0.01116)
//    hid5.InputMap.Add(const5, 0.005) 
//    hid5.InputMap.Add(const6, 0.0005) 
//    hid5.InputMap.Add(const7, -0.06666) 
//    hid5.InputMap.Add(const8, 0.01116) 
//    hid5.InputMap.Add(const9, 0.005) 
//    hid5.InputMap.Add(const10, 0.0005) 
//    hid5.InputMap.Add(const11, 0.06666) 
//    hid5.InputMap.Add(const12, 0.01116) 
//    hid5.InputMap.Add(const13, 0.005) 
//    hid5.InputMap.Add(const14, 0.0005) 
//    hid5.InputMap.Add(const15, 0.06666) 
//    hid5.InputMap.Add(const16, 0.01116)
//
//    let hid6 = new Neuron()
//    hid6.ActivationFunction <- sumOfProducts
//    hid6.OutputFunction <- sigmoid
//    hid6.InputMap.Add(const1, 0.005) 
//    hid6.InputMap.Add(const2, 0.0005) 
//    hid6.InputMap.Add(const3, 0.06666) 
//    hid6.InputMap.Add(const4, 0.01116)
//    hid6.InputMap.Add(const5, 0.005) 
//    hid6.InputMap.Add(const6, 0.0005) 
//    hid6.InputMap.Add(const7, 0.06666) 
//    hid6.InputMap.Add(const8, 0.01116) 
//    hid6.InputMap.Add(const9, 0.005) 
//    hid6.InputMap.Add(const10, 0.0005) 
//    hid6.InputMap.Add(const11, 0.06666) 
//    hid6.InputMap.Add(const12, 0.01116) 
//    hid6.InputMap.Add(const13, 0.005) 
//    hid6.InputMap.Add(const14, 0.0005) 
//    hid6.InputMap.Add(const15, 0.06666) 
//    hid6.InputMap.Add(const16, 0.01116)
//
//    let hid7 = new Neuron()
//    hid7.ActivationFunction <- sumOfProducts
//    hid7.OutputFunction <- sigmoid
//    hid7.InputMap.Add(const1, 0.005) 
//    hid7.InputMap.Add(const2, 0.0005) 
//    hid7.InputMap.Add(const3, 0.06666) 
//    hid7.InputMap.Add(const4, 0.01116)
//    hid7.InputMap.Add(const5, 0.005) 
//    hid7.InputMap.Add(const6, 0.0005) 
//    hid7.InputMap.Add(const7, 0.06666) 
//    hid7.InputMap.Add(const8, 0.01116) 
//    hid7.InputMap.Add(const9, 0.005) 
//    hid7.InputMap.Add(const10, 0.0005) 
//    hid7.InputMap.Add(const11, 0.06666) 
//    hid7.InputMap.Add(const12, 0.01116) 
//    hid7.InputMap.Add(const13, 0.005) 
//    hid7.InputMap.Add(const14, 0.0005) 
//    hid7.InputMap.Add(const15, 0.06666) 
//    hid7.InputMap.Add(const16, 0.01116)
//
//    let hid8 = new Neuron()
//    hid8.ActivationFunction <- sumOfProducts
//    hid8.OutputFunction <- sigmoid
//    hid8.InputMap.Add(const1, 0.005) 
//    hid8.InputMap.Add(const2, 0.0005) 
//    hid8.InputMap.Add(const3, 0.06666) 
//    hid8.InputMap.Add(const4, 0.01116)
//    hid8.InputMap.Add(const5, 0.005) 
//    hid8.InputMap.Add(const6, 0.0005) 
//    hid8.InputMap.Add(const7, 0.06666) 
//    hid8.InputMap.Add(const8, 0.01116) 
//    hid8.InputMap.Add(const9, 0.005) 
//    hid8.InputMap.Add(const10, 0.0005) 
//    hid8.InputMap.Add(const11, 0.06666) 
//    hid8.InputMap.Add(const12, 0.01116) 
//    hid8.InputMap.Add(const13, 0.005) 
//    hid8.InputMap.Add(const14, 0.0005) 
//    hid8.InputMap.Add(const15, 0.06666) 
//    hid8.InputMap.Add(const16, 0.01116)
//
//    let hid9 = new Neuron()
//    hid9.ActivationFunction <- sumOfProducts
//    hid9.OutputFunction <- sigmoid
//    hid9.InputMap.Add(const1, 0.005) 
//    hid9.InputMap.Add(const2, 0.0005) 
//    hid9.InputMap.Add(const3, 0.06666) 
//    hid9.InputMap.Add(const4, 0.01116)
//    hid9.InputMap.Add(const5, 0.005) 
//    hid9.InputMap.Add(const6, 0.0005) 
//    hid9.InputMap.Add(const7, 0.06666) 
//    hid9.InputMap.Add(const8, 0.01116) 
//    hid9.InputMap.Add(const9, 0.005) 
//    hid9.InputMap.Add(const10, 0.0005) 
//    hid9.InputMap.Add(const11, 0.06666) 
//    hid9.InputMap.Add(const12, 0.01116) 
//    hid9.InputMap.Add(const13, 0.005) 
//    hid9.InputMap.Add(const14, 0.0005) 
//    hid9.InputMap.Add(const15, 0.06666) 
//    hid9.InputMap.Add(const16, 0.01116)
//
//    let hidLayer = new NeuralLayer()
//    hidLayer.Add(hid1)
//    hidLayer.Add(hid2)
//    hidLayer.Add(hid3)
//    hidLayer.Add(hid4)
//    hidLayer.Add(hid5)
//    hidLayer.Add(hid6)
//    hidLayer.Add(hid7)
//    hidLayer.Add(hid8)
//    hidLayer.Add(hid9)
//    NN.HiddenLayers.Add(hidLayer)
//
//    let out1 = new Neuron()
//    out1.ActivationFunction <- sumOfProducts
//    out1.OutputFunction <- sigmoid
//    out1.InputMap.Add(hid1, 0.09) 
//    out1.InputMap.Add(hid2, 0.09) 
//    out1.InputMap.Add(hid3, 0.09) 
//    out1.InputMap.Add(hid4, 0.09) 
//    out1.InputMap.Add(hid5, 0.09) 
//    out1.InputMap.Add(hid6, 0.09) 
//    out1.InputMap.Add(hid7, 0.09) 
//    out1.InputMap.Add(hid8, 0.09) 
//    out1.InputMap.Add(hid9, 0.09) 
//
//    let out2 = new Neuron()
//    out2.ActivationFunction <- sumOfProducts
//    out2.OutputFunction <- sigmoid
//    out2.InputMap.Add(hid1, 0.09) 
//    out2.InputMap.Add(hid2, 0.09) 
//    out2.InputMap.Add(hid3, 0.09) 
//    out2.InputMap.Add(hid4, 0.09) 
//    out2.InputMap.Add(hid5, 0.09) 
//    out2.InputMap.Add(hid6, 0.09) 
//    out2.InputMap.Add(hid7, 0.09) 
//    out2.InputMap.Add(hid8, 0.09) 
//    out2.InputMap.Add(hid9, 0.09) 
//
//    NN.OutputLayer.Add("democrat", out1)
//    NN.OutputLayer.Add("republican", out2)


    let algBuilder = new TrainigAlgorithmBuilder.BackPropagationBuilder()
    algBuilder.ParameterStore.SetValue("LEARNING_RATE", Number(0.3))
    algBuilder.ParameterStore.SetValue("EPOCHS", Number(50.0))

    let NN  = new MultiLayerNetwork(algBuilder.BuildTrainingFunction())
    let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\glass.arff"
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

    removeRange [0;1;2;3;4] table
//    removePercentage 55.6 table
//    subsetByExpression "Na>12.0" table

    NN.CreateNetork(table, "RI")          // TODO forse un po' da migliorare l'interfaccia qui
    NN.Train(table, "RI")
    let out = NN.Classify(table.Rows.[0])
    printfn "ACTUAL: %s ---- OUT: %A" (Convert.ToString(table.Rows.[0].["RI"])) out
    let out = NN.Classify(table.Rows.[1])
    printfn "ACTUAL: %s ---- OUT: %A" (Convert.ToString(table.Rows.[1].["RI"])) out
    let out = NN.Classify(table.Rows.[2])                                
    printfn "ACTUAL: %s ---- OUT: %A" (Convert.ToString(table.Rows.[2].["RI"])) out
    let out = NN.Classify(table.Rows.[3])                                
    printfn "ACTUAL: %s ---- OUT: %A" (Convert.ToString(table.Rows.[3].["RI"])) out
    let out = NN.Classify(table.Rows.[4])                                
    printfn "ACTUAL: %s ---- OUT: %A" (Convert.ToString(table.Rows.[4].["RI"])) out


    let valBuilder = new BasicValidationBuilder()
    valBuilder.ParameterStore.SetValue("PERCENTAGE_SPLIT", Number(50.0))
    let stat = NN.Validate(valBuilder.BuildTestTable(table))
    printfn "NumberOfExamples: %d" stat.NumberOfExamples
    printfn "Number Of Correctly Classified Examples: %d" stat.NumberOfCorrectlyClassifiedExamples
    printfn "Percentage of Correctly Classified Examples: %f" ((Convert.ToDouble(stat.NumberOfCorrectlyClassifiedExamples)/Convert.ToDouble(stat.NumberOfExamples))*100.0)
    printfn "Number Of Missclassified Examples: %d" stat.NumberOfMissclassifiedExamples

    let form = new Form()
    let grid = new DataGridView(DataSource=table, Dock=DockStyle.Fill)
    form.Controls.Add(grid)
    form.Visible <- true
    Application.Run(form)


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



    System.Console.ReadLine() |> ignore
    0

   
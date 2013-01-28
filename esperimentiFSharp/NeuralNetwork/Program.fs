module Program

open System
open System.Data
open System.Windows.Forms
open Parameter
open ValidationBuilder

[<EntryPoint>]
let main argv = 
    let table = TableUtilities.buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\vote.arff"
    // TEST - TableUtilities.buildTableFromArff
    let form = new Form()
    form.Text <- table.TableName
    let grid = new DataGridView(DataSource=table, Dock=DockStyle.Fill)
    form.Controls.Add(grid)
    form.Visible <- true
    Application.Run(form)
    0
//
//    let valBuilder = new BasicValidationBuilder()
//    let testTable = valBuilder.BuildTestTable(table)
//    printfn "%A" (table=testTable)                      // Non sono stati settati parametri quindi ls testTable deve essere uguale alla training table (controllo tra riferimenti)
//
//    valBuilder.ParameterStore.SetValue("TEST_SET", String(@"C:\Users\Alessandro\Desktop\cars2004.arff"))
//    let testTable2 = valBuilder.BuildTestTable(table)
////    let form = new Form()
////    let grid = new DataGridView(DataSource=testTable2, Dock=DockStyle.Fill)
////    form.Text <- table.TableName
////    form.Controls.Add(grid)
////    form.Visible <- true
////    Application.Run(form)
//
//    valBuilder.ParameterStore.ClearParameters()
//    valBuilder.ParameterStore.SetValue("PERCENTAGE_SPLIT", Number(10.0))
//    let testTableSplit = valBuilder.BuildTestTable(table)
//    let form = new Form()
//    let grid = new DataGridView(DataSource=testTableSplit, Dock=DockStyle.Fill)
//    form.Text <- table.TableName
//    form.Controls.Add(grid)
//    form.Visible <- true
//    Application.Run(form)
//
////    valBuilder.ParameterStore.SetValue("PERCENTAGE_SPLIT", Number(30.0))        // Settati due parametri -> eccezione
////    let testTable3 = valBuilder.BuildTestTable(table)
//
//    System.Console.ReadLine() |> ignore
//    0
//    
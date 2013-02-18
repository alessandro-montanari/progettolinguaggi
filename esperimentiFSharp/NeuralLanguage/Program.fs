﻿// Learn more about F# at http://fsharp.net
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
open Preprocessing
open Validation
open NetworkIterpreter

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
                                        let net = parser text.Text
                                        console.Text <- sprintf "%A" (net)
                                        let NN, table, stat = evalNetwork net
                                        console.AppendText("================")
                                        stat.PrintStatistcs()
                                        let form = new Form(Visible=true)
                                        let grid = new DataGridView(DataSource=table, Dock=DockStyle.Fill)
                                        form.Controls.Add(grid)
                                    with e ->
                                        console.Text <- e.Message
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
// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

open ArffTypes
open ArffLanguageParser
open ArffLanguageLex
open System.IO

// Define your library scripting code here
//[<EntryPoint>]
//let main argv = 
//    let lexbuf = Lexing.LexBuffer<_>.FromTextReader (new StreamReader(File.OpenRead(@"C:\Users\Alessandro\Desktop\test.arff")))
//    let rec loop token =
//        printfn "%A" token
//        if token <> ArffLanguageParser.EOF then
//            loop (ArffLanguageLex.tokenize lexbuf)
//    loop (ArffLanguageLex.tokenize lexbuf)
//    System.Console.ReadLine() |> ignore
//    0 



[<EntryPoint>]
let main argv = 
    let lexbuf = Lexing.LexBuffer<_>.FromTextReader (new StreamReader(File.OpenRead(@"D:\Users\alessandro\Dropbox\Magistrale\Sistemi Intelligenti\esRetiNeurali\cars2004.arff")))
    try
        printfn "%A" (ArffLanguageParser.start ArffLanguageLex.tokenize lexbuf)
    with e ->
        let pos = lexbuf.EndPos 
        failwithf "Error near line %d, character %d\n" pos.Line pos.Column
    System.Console.ReadLine() |> ignore
    0

//let reader = 
//    seq {   use reader = new StreamReader(File.OpenRead(@"C:\Users\Alessandro\Desktop\test.txt"))
//            while not reader.EndOfStream do
//                yield reader.ReadLine() }
//
//reader
//    |> Seq.iter (fun el -> printfn "%s" el)

    

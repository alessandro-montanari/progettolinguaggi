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
    printfn "%A" (ArffLanguageUtilities.parseFile @"C:\Users\Alessandro\Desktop\test.txt")
    System.Console.ReadLine() |> ignore
    0
    

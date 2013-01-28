module Program

open ArffLanguageParser
open ArffLanguageLex
open System.IO

// Define your library scripting code here
//[<EntryPoint>]
//let main argv = 
//    let lexbuf = Lexing.LexBuffer<_>.FromString ("\"Chevrolet Aveo 4dr\",0,0,0,0,0,0,0, 11690, 10965,1.6, 4,103,28,34,2370, 98,167,66")
//    let rec loop token =
//        printfn "%A" token
//        if token <> ArffLanguageParser.EOF then
//            loop (ArffLanguageLex.tokenize lexbuf)
//    loop (ArffLanguageLex.tokenize lexbuf)
//    System.Console.ReadLine() |> ignore
//    0 


[<EntryPoint>]
let main argv = 
    printfn "%A" (ArffLanguageUtilities.parseFile @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\weather.arff")
    System.Console.ReadLine() |> ignore
    printfn "%A" (ArffLanguageUtilities.parseFile @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\weather.arff")
    System.Console.ReadLine() |> ignore
    0
    

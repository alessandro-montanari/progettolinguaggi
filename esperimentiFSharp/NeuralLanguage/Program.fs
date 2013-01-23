// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open AST
open NeuralLanguageParser
open NeuralLanguageLex

//[<EntryPoint>]
//let main argv = 
//    let lexbuf = Lexing.LexBuffer<_>.FromString " \n ciao:-3.5 , () {} [] ATT3 ATT0 ATT_3 att3 AtT4 < >> >= && || PREPROCESSING sin + - ^ 4 \"ciao ciao\" "
//    let rec loop token =
//        printfn "%A" token
//        if token <> NeuralLanguageParser.EOF then
//            loop (NeuralLanguageLex.tokenize lexbuf)
//        
//
//    loop (NeuralLanguageLex.tokenize lexbuf)
//    System.Console.ReadLine()
//    0 


// Note:
// - p6 non va se attacco il - a 2.0
// - p8 la lista è al contrario
[<EntryPoint>]
let main argv = 
    let lexbuf = Lexing.LexBuffer<_>.FromString "p : -0.7 \n\
                                                 p2 : ATT4+4.0 \n\
                                                 p3 : myatt*-4.9 \n\
                                                 p4 : 5.0 + 6.0 +7.0 \n\
                                                 p4 : 5.0 - 6.0 - 7.0 \n\
                                                 p5 : 5.0 > 6.0 > 7.0 \n\
                                                 p6 : (3.0+5.0)*(9.0 - 2.0) \n\
                                                 p7 : [ciao, ciao, ciao] \n\
                                                 p8 : [ciao, ATT0, ATT6 .. ATT10] \n\
                                                 p9 : [4.0, 5.9, 5.6 .. 90.99] \n\
                                                 p10 : true && false + sin(4.0) \n\
                                                 p11 : 4.5 - 5 \n\
                                                 p12 : [ATT3, \"mio att\", ATT9..ATT20]"
    try
        printfn "%A" (NeuralLanguageParser.start NeuralLanguageLex.tokenize lexbuf)
    with e ->
        let pos = lexbuf.EndPos 
        failwithf "Error near line %d, character %d\n" pos.Line pos.Column
    System.Console.ReadLine()
    0
 


// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open AST
open NeuralLanguageParser
open NeuralLanguageLex

//[<EntryPoint>]
//let main argv = 
//    let lexbuf = Lexing.LexBuffer<_>.FromString "    \n ciao:-3.5 , () {} [] ATT3 ATT0 ATT_3 att3 AtT4 < >> >= && || PREPROCESSING sin + - ^ 4 \"ciao ciao\" IST0"
//    let rec loop token =
//        printfn "%A" token
//        if token <> NeuralLanguageParser.EOF then
//            loop (NeuralLanguageLex.tokenize lexbuf)
//    loop (NeuralLanguageLex.tokenize lexbuf)
//    System.Console.ReadLine() |> ignore
//    0 


// Note:
[<EntryPoint>]
let main argv = 
    let lexbuf = Lexing.LexBuffer<_>.FromString "p17 : mean(ciao), \n\
                                                 p11 : 4.5 + -5 \n\
                                                 PREPROCESSING \n\
                                                 { TRAINING_SET : \"C:\ciao\ciao\ciao.arff\" \n\
                                                    ATTRIBUTE \n\
                                                    { } \n\
                                                    INSTANCE \n\
                                                    { } \n\
                                                  } \n\
                                                  \n\
                                                  MultiLayerNetwork \n\
                                                  { \n\
                                                    p17 : mean(ciao) \n\
                                                    INPUT_LAYER \n\
	                                                { \n\
                                                        p17 : mean(ciao), \n\
                                                        p11 : 4.5 + -5 \n\
                                                    } \n\
                                                    \n\
                                                    HIDDEN_LAYER \n\
                                                    { \n\
                                                        p17 : mean(ciao), \n\
                                                        p11 : 4.5 + -5 \n\
                                                    } \n\
                                                  } \n\
                                                  \n\
                                                  TRAINING BackPropagation \n\
                                                  { \n\
                                                    p17 : mean(ciao), \n\
                                                    p11 : 4.5 + -5, \n\
                                                    p17 : mean(ciao), \n\
                                                    p11 : 4.5 + -5 \n\
                                                  } \n\
                                                  VALIDATION \n\
                                                  { \n\
                                                    TEST_SET : \"C:\\test.arrf\" \n\
                                                    p11 : 4.5 + -5, \n\
                                                    p17 : mean(ciao), \n\
                                                    p11 : 4.5 + -5 \n\
                                                  }" 
    try
        printfn "%A" (NeuralLanguageParser.start NeuralLanguageLex.tokenize lexbuf)
    with e ->
        let pos = lexbuf.EndPos 
        failwithf "Error near line %d, character %d\n" pos.Line pos.Column
    System.Console.ReadLine() |> ignore
    0
 

// let bigTest = "p0 : -0.7 ; \n\
//                p1 : ATT4+4.0; \n\
//                p2 : myatt*-4.9; \n\
//                p3 : 5.0 + 6.0 +7.0; \n\
//                p4 : 5.0 - 6.0 - 7.0; \n\
//                p5 : 5.0 > 6.0 > 7.0; \n\
//                p6 : (3.0+5.0)*(9.0 -2.0); \n\
//                p7 : [ciao, ciao, ciao]; \n\
//                p8 : [ciao, ATT0, ATT6 .. ATT10]; \n\
//                p9 : [4.0, 5.9, 5.6 .. 90.99]; \n\
//                p10 : true && false + sin(4.0); \n\
//                p11 : 4.5 + -5; \n\
//                p12 : [ATT3, \"mio _ * - ^ att\", ATT9..ATT20]; \n\
//                p13 : 5^3^2; \n\
//                p14: !4.5 && !7; \n\
//                p15 : -(3+4*5); \n\
//                p16 : !(5); \n\
//                p17 : mean(ciao); \n\
//                p18 : mean(\"ciao ciao\"); \n\
//                p19 : mean(ATT5); \n\
//                p20 : [INST1, INST3, INST0..INST6, INST7]; \n\
//                p21 : ![ATT4, ATT6] \n\
//                PREPROCESSING \n\
//                { \n\
//                TRAINING_SET : \"C:\ciao\ciao\ciao.arff\" \n\
//                ATTRIBUTE \n\
//                { \n\
//                    filter1(par:6, par2:x+y, par3:ATT3+ATT5); \n\
//                    filter1(par:6, par2:x+y, par3:ATT3+ATT5) \n\
//                } \n\
//                \n\
//                INSTANCE \n\
//                { \n\
//                    filter1(par:6, par2:x+y, par3:ATT3+ATT5); \n\
//                    filter1() \n\
//                }\n\
//                } \n\
//                \n\
//                MultiLayerNetwork WITH_BIAS \n\
//                { \n\
//                p17 : mean(ciao) \n\
//                INPUT_LAYER in \n\
//	            { \n\
//                    p17 : mean(ciao); \n\
//                    p11 : 4.5 + -5 \n\
//                } \n\
//                \n\
//                HIDDEN_LAYER primo \n\
//                { \n\
//                    p17 : mean(ciao); \n\
//                    p11 : 4.5 + -5 \n\
//                } \n\
//                \n\
//                }"
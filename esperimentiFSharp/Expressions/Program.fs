// This project type requires the F# PowerPack at http://fsharppowerpack.codeplex.com/releases
// Learn more about F# at http://fsharp.net
// Original project template by Jomo Fisher based on work of Brian McNamara, Don Syme and Matt Valerio
// This posting is provided "AS IS" with no warranties, and confers no rights.

open System
open Microsoft.FSharp.Text.Lexing

open Ast
open Lexer
open Parser

/// Evaluate a value
let rec evalValue factor =
    match factor with
    | Float x   -> x
    | Integer x -> float x
    | Expression x -> evalExpression x
    | Function (name,ex) -> evalFunction name (evalExpression ex)

and evalFunction name param =
    match name with
    | "sin" -> Math.Sin(param)
    | "cos" -> Math.Cos(param)
    | "tan" -> Math.Tan(param)
    | "atan" -> Math.Atan(param)
    | "sinh" -> Math.Sinh(param)
    | "tanh" -> Math.Tanh(param)
    | "log" -> Math.Log10(param)
    | "ln" -> Math.Log(param)
    | "floor" -> Math.Floor(param)
    | "ceil" -> Math.Ceiling(param)
    | "sqrt" -> Math.Sqrt(param)
    | "exp" -> Math.Exp(param)
    | _ -> failwithf "Function '%s' not supported" name

/// Evaluate an expression
and evalExpression expr =
    match expr with
    | Plus (expr, term)  -> (evalExpression expr) + (evalExpression term)
    | Minus (expr, term) -> (evalExpression expr) - (evalExpression term)
    | Times (term, fact)  -> (evalExpression term) * (evalExpression fact)
    | Divide (term, fact) -> (evalExpression term) / (evalExpression fact)
    | Negative expr -> -(evalExpression expr)
    | Value value -> evalValue value

/// Evaluate an equation
and evalEquation eq =
    match eq with
    | Equation expr -> evalExpression expr

printfn "Calculator"

let rec readAndProcess() =
    printf ":"
    match Console.ReadLine() with
    | "quit" -> ()
    | expr ->
        try
            printfn "Lexing [%s]" expr
            let lexbuff = LexBuffer<char>.FromString(expr)
            
            printfn "Parsing..."
            let equation = Parser.start Lexer.tokenize lexbuff
            
            printfn "Evaluating Equation..."
            let result = evalEquation equation
            
            printfn "Result: %s" (result.ToString())
            
        with ex ->
            printfn "Unhandled Exception: %s" ex.Message

        readAndProcess()

readAndProcess()
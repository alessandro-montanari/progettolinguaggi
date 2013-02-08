// This project type requires the F# PowerPack at http://fsharppowerpack.codeplex.com/releases
// Learn more about F# at http://fsharp.net
// Original project template by Jomo Fisher based on work of Brian McNamara, Don Syme and Matt Valerio
// This posting is provided "AS IS" with no warranties, and confers no rights.

open System
open Microsoft.FSharp.Text.Lexing
open System.Collections.Generic

open Ast
open Lexer
open Parser

let env = new Dictionary<string, double>(HashIdentity.Structural)
env.Add("x", 1.0)
env.Add("y", 1.0)

/// Evaluate a value
let rec evalValue factor (env:Dictionary<string, double>) =
    match factor with
    | Float x   -> x
    | Integer x -> float x
    | Id id -> env.[id]
    | Expression x -> evalExpression x env
    | Function (name,ex) -> evalFunction name (evalExpression ex env)
    | AggregateFunction (name, exList) -> evalAggregateFunction name (exList |> List.map(fun exp -> evalExpression exp env))

and evalAggregateFunction name (paramList: double list) =
    match name with
    | "min" -> paramList |> List.fold (fun prevMin currVal -> Math.Min(prevMin, currVal)) Double.MaxValue

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
and evalExpression expr env =
    match expr with
    | Plus (expr, term)  -> (evalExpression expr env) + (evalExpression term env)
    | Minus (expr, term) -> (evalExpression expr env) - (evalExpression term env)
    | Times (term, fact)  -> (evalExpression term env) * (evalExpression fact env)
    | Pow (term, fact) -> (evalExpression term env) ** (evalExpression fact env)
    | Divide (term, fact) -> (evalExpression term env) / (evalExpression fact env)
    | Negative expr -> -(evalExpression expr env)
    | Value value -> evalValue value env

/// Evaluate an equation
and evalEquation eq env =
    match eq with
    | Equation expr -> evalExpression expr env

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
            let result = evalEquation equation env
            
            printfn "Result: %s" (result.ToString())
            
        with ex ->
            printfn "Unhandled Exception: %s" ex.Message

        readAndProcess()

readAndProcess()
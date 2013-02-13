module Evaluator

open System
open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open ExpressionsAst
open Environment


let parseExpression exp =
    let lexbuff = LexBuffer<char>.FromString(exp)
    try
        Parser.start Lexer.tokenize lexbuff
    with
    | exn -> failwithf "Expression parse error near, line: %d, column: %d" lexbuff.EndPos.Line lexbuff.EndPos.Column

/// Evaluate a value
let rec evalValue factor (env:Environment) =
    match factor with
    | Double x   -> x
    | Id id -> env.EnvSingle.[id]
    | Boolean b -> match b with
                    | true -> 1.0
                    | false -> 0.0
    | Function (name,ex) -> evalFunction name (evalExpression ex env)
    | AggregateFunction (name, exList) -> evalAggregateFunction name (exList |> List.collect(fun exp -> match exp with
                                                                                                        | Value(Id(name)) when env.EnvSeries.ContainsKey(name) -> env.EnvSeries.[name]   // Se mi trovo un ID in una aggregate function, lo vado a cercare in un altro env
                                                                                                        | exp -> [evalExpression exp env]) )

    | SumOfProducts(exList1, exList2) -> List.fold2 (fun acc el1 el2 -> acc + (evalExpression el1 env)*(evalExpression el2 env)) 0.0 exList1 exList2

and evalAggregateFunction name (paramList: double list) =
    match name with
    | "min" -> paramList |> List.fold (fun prevMin currVal -> Math.Min(prevMin, currVal)) Double.MaxValue
    | "max" -> paramList |> List.fold (fun prevMin currVal -> Math.Max(prevMin, currVal)) Double.MinValue
    | "sum" -> paramList |> List.sum
    | "sumsquared" -> paramList |> List.map(fun el -> el*el) |> List.sum
    | "mean" -> paramList |> List.average
    | "sd" ->   let avg = paramList |> List.average
                sqrt (List.fold (fun acc elem -> acc + (float elem - avg) ** 2.0 ) 0.0 paramList / float paramList.Length)     

and evalFunction name param =
    match name with
    | "sin" -> Math.Sin(param)
    | "cos" -> Math.Cos(param)
    | "acos" -> Math.Acos(param)
    | "asin" -> Math.Asin(param)
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
    | "abs" -> Math.Abs(param)

/// Evaluate an expression
and evalExpression expr env =
    let op f v v' = 
        if (f v v') then
            1.0
        else
            0.0
    match expr with
    | Plus (ex1, ex2)  -> (evalExpression ex1 env) + (evalExpression ex2 env)
    | Minus (ex1, ex2) -> (evalExpression ex1 env) - (evalExpression ex2 env)
    | Times (ex1, ex2)  -> (evalExpression ex1 env) * (evalExpression ex2 env)
    | Pow (ex1, ex2) -> (evalExpression ex1 env) ** (evalExpression ex2 env)
    | Divide (ex1, ex2) -> (evalExpression ex1 env) / (evalExpression ex2 env)
    | Negative ex1 -> -(evalExpression ex1 env)
    | Value value -> evalValue value env
    | And(ex1, ex2) -> match (evalExpression ex1 env), (evalExpression ex2 env) with
                        | 1.0,1.0 -> 1.0
                        | _, _ -> 0.0
    | Or(ex1, ex2) -> match (evalExpression ex1 env), (evalExpression ex2 env) with
                        | 1.0,_ | _, 1.0 -> 1.0
                        | _, _ -> 0.0
    | Not ex -> match (evalExpression ex env) with
                | 1.0 -> 0.0
                | _ -> 1.0
    | Lt(ex1, ex2) -> op (<) (evalExpression ex1 env) (evalExpression ex2 env)
    | Lte(ex1, ex2) -> op (<=) (evalExpression ex1 env) (evalExpression ex2 env)
    | Gt(ex1, ex2) -> op (>) (evalExpression ex1 env) (evalExpression ex2 env)
    | Gte(ex1, ex2) -> op (>=) (evalExpression ex1 env) (evalExpression ex2 env)
    | Eq(ex1, ex2) -> op (=) (evalExpression ex1 env) (evalExpression ex2 env)
    | NotEq(ex1, ex2) -> op (<>) (evalExpression ex1 env) (evalExpression ex2 env)



let rec checkValue factor (env:Environment) =
    match factor with
    | Id id -> if not (env.EnvSingle.ContainsKey(id) || env.EnvSeries.ContainsKey(id)) then failwithf "The identifier '%s' is not defined" id
    | Function (name,ex) -> checkExpression ex env
    | AggregateFunction (name, exList) -> exList |> List.iter(fun ex -> checkExpression ex env)
    | SumOfProducts (exList1, exList2) ->   exList1 |> List.iter(fun ex -> checkExpression ex env)
                                            exList2 |> List.iter(fun ex -> checkExpression ex env)
    | _ -> ()  

and checkExpression expr env =
    match expr with
    | Plus (expr, term)  -> checkExpression expr env
                            checkExpression term env
    | Minus (expr, term) -> checkExpression expr env
                            checkExpression term env
    | Times (term, fact)  -> checkExpression term env
                             checkExpression fact env
    | Pow (term, fact) -> checkExpression term env
                          checkExpression fact env
    | Divide (term, fact) -> checkExpression term env
                             checkExpression fact env
    | Negative expr -> checkExpression expr env
    | Value value -> checkValue value env
    | And(ex1, ex2) -> checkExpression ex1 env
                       checkExpression ex2 env
    | Or(ex1, ex2) -> checkExpression ex1 env
                      checkExpression ex2 env
    | Not ex -> checkExpression ex env
    | Lt(ex1, ex2) -> checkExpression ex1 env
                      checkExpression ex2 env
    | Lte(ex1, ex2) -> checkExpression ex1 env
                       checkExpression ex2 env
    | Gt(ex1, ex2) -> checkExpression ex1 env
                      checkExpression ex2 env
    | Gte(ex1, ex2) -> checkExpression ex1 env
                       checkExpression ex2 env
    | Eq(ex1, ex2) -> checkExpression ex1 env
                      checkExpression ex2 env
    | NotEq(ex1, ex2) -> checkExpression ex1 env
                         checkExpression ex2 env



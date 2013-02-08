// This project type requires the F# PowerPack at http://fsharppowerpack.codeplex.com/releases
// Learn more about F# at http://fsharp.net
// Original project template by Jomo Fisher based on work of Brian McNamara, Don Syme and Matt Valerio
// This posting is provided "AS IS" with no warranties, and confers no rights.

open System
open System.Collections.Generic
open System.Data
open System.Windows.Forms

open Microsoft.FSharp.Text.Lexing

open Ast
open Lexer
open Parser
open NeuralTypes
open TableUtilities


type Environment() =
    
    let _envSingle = new Dictionary<string, double>(HashIdentity.Structural)
    let _envSeries = new Dictionary<string, double list>(HashIdentity.Structural)

    member this.EnvSingle = _envSingle
    member this.EnvSeries = _envSeries

let env = new Environment()

let table = buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\glass - Copy.arff"

// Devo controllare che gli identificatori non siano duplicati -> creare una classe che rappresenta l'ambiente
//let envSeries = new Dictionary<string, double list>(HashIdentity.Structural)       

for col in table.Columns do
    let colName = col.ColumnName
    if (table.Columns.[colName] :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then
        let list:double list = query {  for row in table.Select() do
                                        where ( not (row.IsNull(colName)) )
                                        select row.[colName]  } |> Seq.map unbox |> Seq.toList
        env.EnvSeries.Add(col.ColumnName, list)


let rec checkValue factor (env:Environment) =
    match factor with
    | Id id -> if not (env.EnvSingle.ContainsKey(id) || env.EnvSeries.ContainsKey(id)) then failwithf "The identifier '%s' is not defined" id
    | Expression x -> checkExpression x env
    | Function (name,ex) -> checkExpression ex env
    | AggregateFunction (name, exList) -> (exList |> List.iter(fun exp -> checkExpression exp env) )
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

and checkEquation eq env =
    match eq with
    | Equation expr -> checkExpression expr env


/// Evaluate a value
let rec evalValue factor (env:Environment) =
    match factor with
    | Float x   -> x
    | Integer x -> float x
    | Id id -> env.EnvSingle.[id]
    | Expression x -> evalExpression x env
    | Function (name,ex) -> evalFunction name (evalExpression ex env)
    | AggregateFunction (name, exList) -> evalAggregateFunction name (exList |> List.collect(fun exp -> match exp with
                                                                                                        | Value(Id(name)) when env.EnvSeries.ContainsKey(name) -> env.EnvSeries.[name]   // Se mi trovo un ID in una aggregate function, lo vado a cercare in un altro env
                                                                                                        | Value(Range(val1, val2)) ->   if val1 <= val2 then
                                                                                                                                            [val1..val2]
                                                                                                                                        else
                                                                                                                                            [val2..val1]
                                                                                                        | exp -> [evalExpression exp env]) )
    | Range(val1, val2) -> failwith "Ranges can only be evaluated by aggregate functions"
    
                            
                            

and evalAggregateFunction name (paramList: double list) =
    match name with
    | "min" -> paramList |> List.fold (fun prevMin currVal -> Math.Min(prevMin, currVal)) Double.MaxValue
    | "max" -> paramList |> List.fold (fun prevMin currVal -> Math.Max(prevMin, currVal)) Double.MinValue
    | "sum" -> paramList |> List.sum
    | "sumsquared" -> paramList |> List.map(fun el -> el*el) |> List.sum
    | "mean" -> paramList |> List.average
    | "sd" ->   let avg = paramList |> List.average
                sqrt (List.fold (fun acc elem -> acc + (float elem - avg) ** 2.0 ) 0.0 paramList / float paramList.Length)      //TODO Manca sumOfProducts(range1, range2)

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
//    let exp = "min([RI])-Na"
//    printfn "%s" exp
//    
//    let lexbuff = LexBuffer<char>.FromString(exp)
//    let equation = Parser.start Lexer.tokenize lexbuff
//
//    let attName = "NEW"
//
//    let colNameList = [for col in table.Columns do yield col.ColumnName]
//    colNameList
//    |> List.iter(fun el -> env.EnvSingle.Add(el, 0.0))    
//    checkEquation equation env
//
//    env.EnvSingle.Clear()
//
//    let column = new AttributeDataColumn(AttributeType.Numeric)           // Tutto in parallelo/asincrono
//    column.DataType <- typeof<double>
//    column.ColumnName <- attName
//    table.Columns.Add(column)
//    for row in table.Select() do
//        for col in table.Columns do
//            let colName = col.ColumnName
//            if colName <> attName && not(row.IsNull(colName)) && (table.Columns.[colName] :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then
//                env.EnvSingle.Add(colName, unbox row.[colName])
//        row.[attName] <- try
//                            box (evalEquation equation env)          // Qui potrei andare parallelo/asincrono
//                         with
//                         | exn -> box DBNull.Value
//        env.EnvSingle.Clear()
//
//
//    let form = new Form()
//    let grid = new DataGridView(Dock=DockStyle.Fill, DataSource=table)
//    form.Controls.Add(grid)
//    form.Visible <- true
//    Application.Run(form)
   

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
// This project type requires the F# PowerPack at http://fsharppowerpack.codeplex.com/releases
// Learn more about F# at http://fsharp.net
// Original project template by Jomo Fisher based on work of Brian McNamara, Don Syme and Matt Valerio
// This posting is provided "AS IS" with no warranties, and confers no rights.

open System
open System.Collections.Generic
open System.Data
open System.Windows.Forms

open Microsoft.FSharp.Text.Lexing

open ExpressionsAst
open Lexer
open Parser
open NeuralTypes
open TableUtilities

// Mi serve un'Environment "doppio" per tenere i valori scalari e le liste di valori
type Environment() =
    
    let _envSingle = new Dictionary<string, double>(HashIdentity.Structural)
    let _envSeries = new Dictionary<string, double list>(HashIdentity.Structural)

    member this.EnvSingle = _envSingle
    member this.EnvSeries = _envSeries




let env = new Environment()

let table = buildTableFromArff @"C:\Users\Alessandro\Dropbox\Magistrale\Linguaggi\Progetto\DataSet\glass - Copy.arff"

for col in table.Columns do
    let colName = col.ColumnName
    if (table.Columns.[colName] :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then         //TODO devo lasciar passare anche gli attributi stringa
        let list:double list = query {  for row in table.Select() do
                                        where ( not (row.IsNull(colName)) )
                                        select row.[colName]  } |> Seq.map unbox |> Seq.toList
        env.EnvSeries.Add(col.ColumnName, list)


let rec checkValue factor (env:Environment) =
    match factor with
    | Id id -> if not (env.EnvSingle.ContainsKey(id) || env.EnvSeries.ContainsKey(id)) then failwithf "The identifier '%s' is not defined" id
//    | Expression x -> checkExpression x env
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
                                               

printfn "Calculator"


let rec readAndProcess() =
//    let exp = "RI>1.5"
//    printfn "%s" exp
//    
//    let lexbuff = LexBuffer<char>.FromString(exp)
//    let expression = Parser.start Lexer.tokenize lexbuff
//
//    let attName = "NEW"
//
//    let colNameList = [for col in table.Columns do yield col.ColumnName]
//    colNameList
//    |> List.iter(fun el -> env.EnvSingle.Add(el, 0.0))    
//    checkExpression expression env
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
//                            box (evalExpression expression env)          // Qui potrei andare parallelo/asincrono
//                         with
//                         | exn -> box DBNull.Value
//        env.EnvSingle.Clear()
//
//
//
//    for row in table.Select() do
//        for col in table.Columns do
//            let colName = col.ColumnName
//            if  not(row.IsNull(colName)) && (table.Columns.[colName] :?> AttributeDataColumn).AttributeType = AttributeType.Numeric then
//                env.EnvSingle.Add(colName, unbox row.[colName])
//        let testRes = try
//                        Some(evalExpression expression env)
//                      with
//                      | exn -> None                         // Vado qui se per un attributo specificato nell'espressione c'è un missing (non trova l'ID nell'env)
//        match testRes with
//        | Some(1.0) -> table.Rows.Remove(row)
//        | Some(_) | None -> ()                              // Se l'espressione è false o un valore è missing, non elimino la riga
//
//        env.EnvSingle.Clear()
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
            let expression = Parser.start Lexer.tokenize lexbuff
            
            printfn "Evaluating Equation..."
            let result = evalExpression expression env
            
            printfn "Result: %s" (result.ToString())
            
        with ex ->
            printfn "Unhandled Exception: %s" ex.Message

        readAndProcess()

readAndProcess()
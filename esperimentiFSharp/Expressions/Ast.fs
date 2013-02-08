namespace Ast
open System

type Value =
    | Float   of Double
    | Integer of Int32
    | Id of string
    | Expression of Expression
    | Function of string * Expression
    | AggregateFunction of string * Expression list

and Expression =
    | Value of Value
    | Negative of Expression
    | Times  of Expression * Expression
    | Pow  of Expression * Expression
    | Divide of Expression * Expression
    | Plus  of Expression * Expression
    | Minus of Expression * Expression

and Equation =
    | Equation of Expression
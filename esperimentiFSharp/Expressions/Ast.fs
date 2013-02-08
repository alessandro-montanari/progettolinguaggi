namespace Ast
open System

type Value =
    | Float   of Double
    | Integer of Int32
    | Expression of Expression
    | Function of string * Expression

and Expression =
    | Value of Value
    | Negative of Expression
    | Times  of Expression * Expression
    | Divide of Expression * Expression
    | Plus  of Expression * Expression
    | Minus of Expression * Expression

and Equation =
    | Equation of Expression
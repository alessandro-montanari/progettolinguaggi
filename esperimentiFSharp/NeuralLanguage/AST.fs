module AST

type Relation =
    | LT
    | LTE
    | GT
    | GTE
    | EQ
    | NotEQ

type Expression =
    | True
    | False
    | And of Expression * Expression
    | Or of Expression * Expression
    | Not of Expression
    | Rel of Expression * Relation * Expression
    | Id of string
    | Neg of Expression
    | Num of double
    | Add of Expression * Expression
    | Sub of Expression * Expression
    | Prod of Expression * Expression
    | Frac of Expression * Expression
    | Pow of Expression * double
    | Sin of Expression
    | Cos of Expression
    | Tan of Expression
    | Atan of Expression
    | Log of Expression
    | Ln of Expression
    | Floor of Expression
    | Ceil of Expression
    | Sqrt of Expression
    | Mean of 

type ParameterValue =
    | AttList of string list
    | NumLis of double list
    | Val of Expression

type Parameter = string * ParameterValue

type Filter = string * Parameter list option

type Aspect = string * string * Parameter list option

type Network =
    {
        Directives : Parameter list option;
        Preprocessing : Filter list * Filter list;
        NetworkDefinition : string * bool * Parameter list option * Aspect list option;
        Training : string * Parameter list option;
        Validation : (string * Parameter list option) option;   // Forse qui conviene fattorizzare in un tipo Validation = string * Parameter list option
    }
    
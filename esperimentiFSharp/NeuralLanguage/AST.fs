module AST

// Pensare se servono dei membri nei vari tipi

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
    | Mean of string
    | Sd of string
    | Min of string
    | Max of string
    | Sum of string
    | SumSquared of string

type ParameterValue =
    | AttList of string list
    | NumLis of double list
    | Val of Expression

type Parameter = string * ParameterValue

type Filter = string * Parameter list

type Aspect = string * string * Parameter list

type Network =
    {
        Directives : Parameter list;
        Preprocessing : Filter list * Filter list;
        NetworkDefinition : string * bool * Parameter list * Aspect list;
        Training : string * Parameter list;
        Validation : (string * Parameter list) option;   // Forse qui conviene fattorizzare in un tipo Validation = string * Parameter list option
    }
    
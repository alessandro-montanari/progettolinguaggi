module AST

// Pensare se servono dei membri nei vari tipi

type Relation =
    | Lt
    | Lte
    | Gt
    | Gte
    | Eq
    | NotEq

type Expression =
    | True
    | False
    | And of Expression * Expression
    | Or of Expression * Expression
    | Not of Expression
    | Rel of Expression * Relation * Expression
    | Id of string
    | AttId of string
    | Neg of Expression
    | Num of double
    | Add of Expression * Expression
    | Sub of Expression * Expression
    | Prod of Expression * Expression
    | Frac of Expression * Expression
    | Pow of Expression * Expression
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

// La sequenza di attributi o di numeri viene espansa in fase di valutazione, quindi
// si memorizza come tupla di stringhe o di double
type AttributeSequence = string * string
type NumberSequence = double * double

type AttributeListElement =
    | Id of string
    | AttId of string
    | Seq of AttributeSequence

type NumberListElement =
    | Num of double
    | Seq of NumberSequence

type ParameterValue =
    | AttList of AttributeListElement list
    | NumList of NumberListElement list
    | Val of Expression

type Parameter = string * ParameterValue

type Filter = string * Parameter list

type Aspect = string * string * Parameter list

type Network =
    {
        Directives : Parameter list;
        Preprocessing : Filter list * Filter list;              // filtri di attributo e di istanza
        NetworkDefinition : string * bool * Parameter list * Aspect list;
        Training : string * Parameter list;
        Validation : (string * Parameter list) option;   // Forse qui conviene fattorizzare in un tipo Validation = string * Parameter list option
    }
    
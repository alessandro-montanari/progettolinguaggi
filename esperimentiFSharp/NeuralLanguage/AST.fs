module AST

//open ExpressionsAst

type Value =
    | Boolean of bool
    | Double   of double
    | Id of string
    | Function of string * Expression
    | AggregateFunction of string * Expression list
    | SumOfProducts of Expression list * Expression list                        

and Expression =
    | Value of Value
    | Negative of Expression
    | Not of Expression
    | Times  of Expression * Expression
    | Pow  of Expression * Expression
    | Divide of Expression * Expression
    | Plus  of Expression * Expression
    | Minus of Expression * Expression
    | And of Expression * Expression
    | Or of Expression * Expression
    | Lt of Expression * Expression
    | Lte of Expression * Expression
    | Gt of Expression * Expression
    | Gte of Expression * Expression
    | Eq of Expression * Expression
    | NotEq of Expression * Expression

type InstanceListElement = 
    | InstIndex of int
    | InstSequence of int * int

type AttributeListElement =
    | AttName of string
    | AttIndex of int
    | AttSequence of int * int

type NumberListElement =
    | Exp of Expression
    | NumberSequence of Expression * Expression

type ParameterValue =
    | AttList of bool * AttributeListElement list
    | NumList of bool * NumberListElement list
    | InstList of bool * InstanceListElement list
    | String of string                                      // C'è Id anche in Expression ma quello denota un valore double, questo è proprio una stringa
    | Exp of Expression                                     // O stringa e poi chi la usa invoca il valutatore di Expressions??

type Parameter = Parameter of string * ParameterValue
type Filter = Filter of string * Parameter list
type Aspect = Aspect of string * Parameter list

type Network =
    {
        Directives : Parameter list;
        TrainingSet: string
        ClassAttribute : string
        Preprocessing : Filter list * Filter list;              // filtri di attributo e di istanza
        NetworkDefinition : string * Parameter list * Aspect list;
        Training : string * Parameter list * Aspect list;
        Validation : Parameter list * Aspect list;                                
    }


// Utilizzo le Discriminated Unions anche dove non sarebbero strettamente necessarie (Parameter, Filter, Aspect)
// perché così i pattern matching saranno più leggibili, del tipo:
// match p with
//    | Parameter(name, value) -> printfn "%s - %A" name value

//let p = Parameter("ciao", AttList (true, [Id "ciao"; AttributeSequence(0, 5); AttId 7]))   
//let p2 = Parameter("ciao2", NumList(false, [Num 4.0; Num 5.9; NumberSequence(4.5, 5.6)]))
//let p3 = Parameter("ciao3", Exp (Add(ExpNum 4.5, ExpNum 5.6)))
//let ex1 = Add(Prod(ExpId "ciao", ExpId "ciao2"), Sub(ExpNum 4.5, Sin (ExpNum 5.8)))
//let f1 = [Filter("f1",[p; p2; p3])]
//let net = { 
//            Directives = [p; p3]; 
//            Preprocessing = ("data.arff", f1, [Filter("f2", [p2;p3])]);
//            NetworkDefinition = ("net", true, [p3], [Aspect("a2", [p2;p3])])
//            Training = ("tra", [p; p2; p3]);
//            Validation = Some("ciao",  [p; p3]) 
//          }
//
//match f1 with
//    | Filter(_, plist) :: _ -> match plist with
//                                    | Parameter(name, exp) :: _ when name = "ciao" -> printfn "%s --- %A" name exp
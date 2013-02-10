namespace Ast
open System

// Note:
// - Range: a livello di parser e AST si poteva anche fare in modo che si potesse usare solo con le aggregate functions, così ci si risparmiava il 
// "failwith "Ranges can only be evaluated by aggregate functions"" nell'interprete. Però si complicava il parser e l'ast
// - Funzioni: si poteva generalizzare l'invocazione di una funzione (con nome, parametri formali, parametri attuali, ecc.) rendendo la grammatica molto generale.
//      La grammatica sarebbe diventata come la grammatica del C per le chiamate di funzioni. Sicuramente diventava il tutto più flessibile (era più facile inserire nuove
//      funzioni anche con un numero di parametri maggiore di 2/2 senza dover modificare la grammatica), però questo avrebbe comportato un complicazione lato interprete.
//      Si è ritenuto non complicare l'interprete più di tanto visto l'obbiettivo di voler rappresentare delle semplici funzioni matematiche.
// - Espressioni booleane: valori double come boolean. 1.0 -> true, 0.0 -> false

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



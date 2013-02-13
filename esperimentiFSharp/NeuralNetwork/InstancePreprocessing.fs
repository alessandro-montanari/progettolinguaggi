module InstancePreprocessing

open System
open System.Data
open Environment
open Evaluator
open PreprocessingUtilities

let removeRange (instances:int list) (table:DataTable) =
    instances
    |> List.map (fun index -> table.Rows.[index])
    |> List.iter (fun row -> table.Rows.Remove(row) )

let removePercentage (percentage:double) (table:DataTable) =
    if percentage < 0.0 || percentage > 100.0 then
        failwith "'percentage' must be in the range [0.0, 100.0]"

    let numRows = Convert.ToInt32(float table.Rows.Count * (float percentage/100.0))
    let maxRows = table.Rows.Count-1
    let random = new Random()
    let rec loop target set =                                       // per arrivare al numero di row distinte richieste
        match target with
        | 0 -> set
        | _ ->  let set2 = [for i in 0..target-1 do yield random.Next(0, maxRows)] |> Set.ofList
                let union = Set.union set set2
                loop (abs(numRows-union.Count)) union 

    let list = loop numRows Set.empty<int>
    removeRange (Set.toList list) table

let subsetByExpression expression (table:DataTable) =
    let exp = parseExpression expression
    let env = new Environment()

    checkExpression table exp []

    fillAttributesSeries env table

    for row in table.Select() do
        fillAttributesSingle env row
        let testRes = try
                        Some(evalExpression exp env)
                      with
                      | exn -> None                         // Vado qui se per un attributo specificato nell'espressione c'è un missing (non trova l'ID nell'env)
        match testRes with
        | Some(1.0) -> table.Rows.Remove(row)
        | Some(_) | None -> ()                              // Se l'espressione è false o un valore è missing, non elimino la riga
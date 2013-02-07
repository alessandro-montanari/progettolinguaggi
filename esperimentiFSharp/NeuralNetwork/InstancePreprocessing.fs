module InstancePreprocessing

open System
open System.Data

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

let subsetByExpression (expression:string) (table:DataTable) =
    let expression = "not("+expression+")"
    let rowsToDelete = table.Select(expression)
    rowsToDelete
    |> Array.iter (fun row -> table.Rows.Remove(row) )
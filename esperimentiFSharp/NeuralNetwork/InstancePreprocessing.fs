module InstancePreprocessing

open System
open System.Data

let removeRange (instances:int list) (dt:DataTable) =
    instances
    |> List.map (fun index -> dt.Rows.[index])
    |> List.iter (fun row -> dt.Rows.Remove(row) )

let removePercentage (percentage:double) (dt:DataTable) =
    let numRows = Convert.ToInt32(float dt.Rows.Count * (float percentage/100.0))
    let maxRows = dt.Rows.Count-1
    let random = new Random()
    let rec loop target set =                                       // per arrivare al numero di row distinte richieste
        match target with
        | 0 -> set
        | _ ->  let set2 = [for i in 0..target-1 do yield random.Next(0, maxRows)] |> Set.ofList
                let union = Set.union set set2
                loop (abs(numRows-union.Count)) union 

    let list = loop numRows Set.empty<int>
    removeRange (Set.toList list) dt

let subsetByExpression (expression:string) (dt:DataTable) =
    let expression = "not("+expression+")"
    let rowsToDelete = dt.Select(expression)
    rowsToDelete
    |> Array.iter (fun row -> dt.Rows.Remove(row) )
//module FileUtilities

open System.Data
open System.IO
open System.Text.RegularExpressions


let table = new DataTable()
let reader = 
    seq {   use reader = new StreamReader(File.OpenRead(@"D:\Users\alessandro\Dropbox\Magistrale\Sistemi Intelligenti\esRetiNeurali\cars2004.arff"))
            while not reader.EndOfStream do
                yield reader.ReadLine() }
reader 
    |> Seq.filter (fun el -> not(el.StartsWith("%") || el = ""))
    |> Seq.truncate 40
    |> Seq.iter (fun el ->
                            if el.StartsWith("@relation") then
                                let m = Regex.Match(el, "@attribute")
                                printfn "%s" m.Value
                            elif el.StartsWith("@attribute") then
                                let elSplitted = el.Split([| "@attribute" |], System.StringSplitOptions.RemoveEmptyEntries)
                                printfn "%A" elSplitted
                            )


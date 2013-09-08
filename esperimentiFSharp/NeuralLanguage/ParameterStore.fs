module ParameterStore

open System
open System.Collections.Generic

type ParameterStore(rules : Dictionary<string, (string -> obj -> unit)>) =
    
    let _params = new Dictionary<string, ResizeArray<obj>>()

    /// Names of the parameters that can be setted in this store
    member this.ParameterNames = rules.Keys |> Seq.readonly

    /// Returns the sequence of values for each parameter in the store
    member this.ParameterValues = _params |> Seq.map (fun el -> el.Key,el.Value|>Seq.readonly)  // la lista di oggetti (el.Value) viene resa readOnly così assume anche il tipo seq<obj>

    /// Add a new value for the specified parameter. An exception is raised if it is not possible to set the specified parameter 
    /// or if the constraint on the value is not satisfied
    member this.AddValue(paramName, newValue) =
        let checkFun = match rules.TryGetValue(paramName) with
                        | res,checkFun when res=true -> checkFun
                        | _ -> failwithf "The parameter '%s' cannot be set in the parameterstore" paramName
        checkFun paramName newValue                               // Se passa questo punto, il check è andato a buon fine, posso inserire il valore
        match _params.TryGetValue(paramName) with
        | res, objList when res=true -> objList.Add(newValue)
        | res, _ when res=false-> let list = new ResizeArray<obj>()
                                  list.Add(newValue)
                                  _params.Add(paramName, list)

    /// Returns the list of values for the specified parameter
    member this.GetValues(paramName) =
        match _params.TryGetValue(paramName) with
        | res, objlist when res=true -> objlist |> Seq.readonly
        | _,_ -> Seq.empty

    /// Clear all the values for all parameters
    member this.Clear() =
        _params.Clear()

// TEST
//let rules = new Dictionary<string, (string->obj->unit)>(HashIdentity.Structural)
//rules.Add("param1", (fun name input -> ()))
//rules.Add("param2", (fun name input -> (if input.GetType() <> typeof<double> then invalidArg name "wrong type")))
//let paramStore = new ParameterStore(rules)
//paramStore.GetValues("param1")
//paramStore.ParameterValues
//paramStore.ParameterNames
//paramStore.AddValue("param1", new Dictionary<string, (obj->unit)>(HashIdentity.Structural))
//paramStore.AddValue("param2", 5.7)
//paramStore.AddValue("param2", 5)
//paramStore.AddValue("param3", 5.9)
//paramStore.Clear()
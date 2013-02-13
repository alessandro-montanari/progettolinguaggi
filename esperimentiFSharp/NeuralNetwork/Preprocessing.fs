module Preprocessing

open System
open System.Reflection
open System.Collections.Generic

let private assembly = Assembly.GetExecutingAssembly()
let private instanceModule  = assembly.GetType("InstancePreprocessing")
let private attributeModule  = assembly.GetType("AttributePreprocessing")

let private instanceFunctions = new Dictionary<string, ParameterInfo list>(HashIdentity.Structural)
instanceModule.GetMethods()
|> Array.iter(fun met -> instanceFunctions.Add(met.Name, met.GetParameters() |> Array.toList))

let private attributeFunctions = new Dictionary<string, ParameterInfo list>(HashIdentity.Structural)
attributeModule.GetMethods()
|> Array.iter(fun met -> attributeFunctions.Add(met.Name, met.GetParameters() |> Array.toList))

let private invoke (theModule : Type) (funcsDict:Dictionary<string, ParameterInfo list>) funcName (paramValues : Dictionary<string, obj>)=
    match funcsDict.TryGetValue(funcName) with
    | res, paramList when res=true ->   if paramList.Length <> paramValues.Count then
                                            failwithf "Parameter count mismatch for '%s'" funcName
                                        // Prima riordino i parametri...
                                        let values = paramList                                                                      
                                                        |> List.map(fun par -> match paramValues.TryGetValue(par.Name) with
                                                                                | res, value when res=true -> value             
                                                                                | _,_ -> failwithf "the parameter '%s' of '%s' must be setted" par.Name funcName)      // Check semantico per controllare che tutti i parametri siano settati
                                                        |> List.toArray

                                        // ... e poi invoco
                                        try
                                            let met = theModule.GetMethod(funcName)
                                            met.Invoke(null, values) |> ignore
                                        with
                                            | :? ArgumentException -> failwithf "The parameters passed to '%s' are not correct" funcName 
                                            // Non catturo qui la TargetInvocationException, tanto verrà catturata da chi invoca me
    | _,_ -> failwithf "The filter '%s' is not defined in '%s'" funcName theModule.Name

let invokeInstanceFilter funcName paramValues =
    invoke instanceModule instanceFunctions funcName paramValues

let invokeAttributeFilter funcName paramValues =
    invoke attributeModule attributeFunctions funcName paramValues

let attributeFilters =
    attributeFunctions.Keys |> Seq.toList

let instanceFilters =
    instanceFunctions.Keys |> Seq.toList
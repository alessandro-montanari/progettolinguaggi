module Builder

open System
open System.Collections.Generic
open Parameter

/// Every builder is able to build an object of type 'T configured via global parameters or aspects (collections of parameters).
/// The constructor takes the cheking rules for the global parameters and the set of rules for each parameter in each aspect
[<AbstractClass>]
type Builder<'T>(globalRules:Dictionary<string, (string->obj->unit)>, aspectRules:Dictionary<string, Dictionary<string,(string->obj->unit)>> ) =
    
    let _globParameterStore = new ParameterStore(globalRules)
    let _aspects = new Dictionary<string, ResizeArray<ParameterStore>>(HashIdentity.Structural)

    /// Add the specified paramters to the specified aspect. An exception is raised if the aspect cannot be set in this builder or if the parameters doesn't pass the rules
    member this.AddAspect(aspectName, parameters:(string*obj) list) =
        let aspectRule = match aspectRules.TryGetValue(aspectName) with
                            | res, rule when res=true -> rule
                            | _ -> failwithf "The aspect %s cannot be set in the builder" aspectName
        // Ho trovato le regole per questo aspetto adesso costruisco il ParameterStore e inserisco i valori
        let paramStore = new ParameterStore(aspectRule)
        parameters
        |> List.iter (fun (name, value) -> paramStore.AddValue(name, value))        // I valori potrebbero non essere validi, sono controllati dalle regole inserite nel paramStore

        // Inserisco il nuovo aspect nel builder
        match _aspects.TryGetValue(aspectName) with
        | res, storeList when res=true -> storeList.Add(paramStore)
        | res, _ when res=false ->  let list = new ResizeArray<ParameterStore>()
                                    list.Add(paramStore)
                                    _aspects.Add(aspectName, list)

    member this.GlobalParameters = _globParameterStore

    /// Returns a sequence of ParameterStore for each aspect currently in the builder
    member this.Aspects =  _aspects |> Seq.map (fun el -> el.Key,el.Value|>Seq.readonly)

    /// Names of the aspects that can be setted in the builder
    member this.AspectsNames = aspectRules.Keys |> Seq.readonly

    /// Returns the sequence of ParameterStore for the specified aspect
    member this.GetAspects(aspectName) =
        match _aspects.TryGetValue(aspectName) with
        | res, list when res=true -> list |> Seq.readonly
        | _,_ -> Seq.empty

    /// Clear all the values for all aspects
    member this.ClearAspects() =
        _aspects.Clear()

    abstract member Name : string
    abstract member Build : unit -> 'T


// TEST
//let globalRules = new Dictionary<string, (string->obj->unit)>(HashIdentity.Structural)
//globalRules.Add("param1", (fun name input -> ()))
//globalRules.Add("param2", (fun name input -> (if input.GetType() <> typeof<double> then invalidArg name "wrong type")))
//
//let aspectsRules = new Dictionary<string, Dictionary<string,(string->obj->unit)>>(HashIdentity.Structural)
//aspectsRules.Add("hiddenLayer", globalRules)
//aspectsRules.Add("outputLayer", globalRules)
//
//let myBuilder = { new Builder<double>(globalRules, aspectsRules) with
//                    override this.Name = "MyBuilder"
//                    override this.Build() =
//                            4.5                           }
//myBuilder.AddAspect("hiddenLayer", [("param1",box 5);("param1",box 6);("param2",box 5.6)])
//myBuilder.AddAspect("hiddenLayer", [("param1",box 5);("param1",box 6);("param2",box 5.6)])
//myBuilder.AddAspect("outputLayer", [("param1",box 5);("param1",box 6);("param2",box 5.6)])
//myBuilder.AddAspect("outputLayer", [("param1",box 5);("param1",box 6);("param2",box 5.6)])
//myBuilder.AddAspect("hiddenLayer", [("param1",box 5);("param1",box 6);("param2",box 5)])
//myBuilder.AddAspect("hiddenLayerz", [("param1",box 5);("param1",box 6);("param2",box 5.6)])
//myBuilder.ClearAspects()
//myBuilder.GetAspects("hiddenLayer")
//myBuilder.Aspects
//myBuilder.AspectsNames


    
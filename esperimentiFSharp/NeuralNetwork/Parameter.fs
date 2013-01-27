module Parameter

open System
open System.Collections.Generic

// Idea:
// - il framework è indipendente dal linguaggio
// - i tipi dei parametri che possono essere settati nel framework sono ovviamente simili a quelli del linguaggio ma sono molto meno dettagliati di quelli dell'AST
// - per le espressioni, non si setta l'espressione in se (stringa) ma il suo risultato, quindi double o boolean. In pratica l'interprete del linguaggio quando incontra un'espression
// la valuta e setta il valore nel builder

type ParameterValue =
    | AttributeList of string list
    | NumberList of double list
    | InstanceList of int list
    | String of string
    | Number of double
    | Logic of bool

type ParameterStore(typeDic : Dictionary<string, Type>) =

    let _paramsDict = new Dictionary<string, ParameterValue>(HashIdentity.Structural)
    let _paramsTypeDict : Dictionary<string, Type> = typeDic

    member this.ParameterNames = _paramsTypeDict.Keys |> Seq.readonly
    member this.ParameterValues = _paramsDict.Values |> Seq.readonly
    member this.Parameters = _paramsDict |> Seq.readonly

    member this.SetValue(paramName, newValue) =
        let attType = _paramsTypeDict.[paramName]
        if newValue.GetType() <> attType then
            invalidArg "newValue" "Invalid argument type"
        else
            _paramsDict.[paramName] <- newValue        

    member this.GetValue(paramName) =
        _paramsDict.[paramName]

    member this.ClearParameters() =
        _paramsDict.Clear() 
module Parameter

open System
open System.Collections.Generic

// Idea:
// - il framework è indipendente dal linguaggio
// - i tipi dei parametri che possono essere settati nel framework sono ovviamente simili a quelli del linguaggio ma sono molto meno dettagliati di quelli dell'AST
// - TODO per le espressioni, non si setta l'espressione in se (stringa) ma il suo risultato, quindi double o boolean. In pratica l'interprete del linguaggio quando incontra 
// un'espressione la valuta e setta il valore nel builder

type ParameterValue =
    | AttributeList of string list
    | NumberList of double list
    | InstanceList of int list
    | String of string
    | Number of double
    | Logic of bool

    member self.NumberOf =
        match self with
        | Number(n) -> n
        | _ -> failwith "Not a number"

//TODO Possibilita di impostare insieme al tipo valore anche una funzione che controlli che il valore settato sia corretto (ParameterValue -> bool)
type ParameterStore(typeDic : Dictionary<string, (Type*(ParameterValue -> bool))>) =
    
    let _paramsDict = new Dictionary<string, ParameterValue>(HashIdentity.Structural)
    let _paramsTypeDict : Dictionary<string, (Type*(ParameterValue -> bool))> = typeDic

    let checkType expected actual =
        if expected <> actual then
            invalidArg "newValue" "Invalid argument type"
        else 
            true
    
    let checkConstraint constraintFunction actualValue =
        if constraintFunction actualValue then
            true
        else
            invalidArg "newValue" "The value does not satisfy the constraint on this parameter"

    member this.ParameterNames = _paramsTypeDict.Keys |> Seq.readonly
    member this.ParameterValues = _paramsDict.Values |> Seq.readonly
    member this.Parameters = _paramsDict |> Seq.readonly

    member this.SetValue(paramName, newValue) =
        let attType, constraintFun = _paramsTypeDict.[paramName]
        let actualType = newValue.GetType()

        if (checkType attType actualType) && (checkConstraint constraintFun newValue) then
            _paramsDict.[paramName] <- newValue        

    member this.GetValue(paramName) =
        _paramsDict.[paramName]

    member this.ClearParameters() =
        _paramsDict.Clear() 
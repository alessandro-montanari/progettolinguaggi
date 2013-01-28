module NeuralTypes

open System
open System.Collections.Generic

type AttributeType =
    | String
    | Numeric
    | Nominal of string list

type AttributeValue =
    | String of string
    | Nominal of string * int
    | Numeric of double
    | Missing

    override this.ToString() =      // Così ho una visualizzazione decente nelle DataGridView
        match this with
        | Numeric(n) -> Convert.ToString(n)
        | String(s) -> s
        | Nominal(s,_) -> s
        | Missing -> "MISS"

type Attribute = Attribute of string * AttributeType

type Instance = Instance of AttributeValue list

type DataSet = 
    {
        Relation : string
        Attributes : Attribute list
        Data : seq<Instance>
    }

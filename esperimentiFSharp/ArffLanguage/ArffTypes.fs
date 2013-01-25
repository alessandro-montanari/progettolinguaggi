module ArffTypes

type ArffType =
    | String
    | Numeric
    | Nominal of string list

type ArffValue =
    | String of string
    | Numeric of double
    | Missing

type ArffAttribute = ArffAttribute of string * ArffType

type ArffInstance = ArffInstance of ArffValue list

type ArffData = seq<ArffInstance>

type ArffFile = 
    {
        Relation : string
        Attributes : ArffAttribute list
        Data : ArffData
    }

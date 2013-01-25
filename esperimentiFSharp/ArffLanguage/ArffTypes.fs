module ArffTypes

// Non unifico questi tipi con quelli della rete neurale (ad esempio OutputValue)
// perché tanto le due cose sono distinte e possono rimanere tali

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


// per il testing
let arffFile =
    {
        Relation = "cars";
        Attributes = [ArffAttribute("att1", ArffType.String); ArffAttribute("att2", ArffType.Numeric); ArffAttribute("att3", Nominal(["a";"b";"c"]))];
        Data = Seq.init 5 (fun _ -> ArffInstance[ArffValue.String("val1"); ArffValue.Missing; ArffValue.Numeric(4.5)])
    }
module NeuralTypes

open System
open System.Collections.Generic

type ActivationFunType = seq<double * double> -> double   // input * peso
type OutputFunType = double -> double

type AttributeType =
    | String
    | Numeric
    | Nominal of string list

type AttributeValue =
    | String of string
    | Nominal of string * int
    | Numeric of double
    | Missing

    member self.IsNumber =
        match self with 
        | Numeric _ -> true
        | _ -> false

    member self.NumberOf =
        match self with
        | Numeric v -> v
        | _ -> failwith "Not a number"

    override this.ToString() =     
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

//let str = String("ciao")
//let str2 = String("ciaso")
//str>str2
//
//let nom1 = Nominal("ciao",1)
//let nom2 = Nominal("ciao",2)
//nom1>nom2
//
//type MyType =
//    | Num of double
//    | Num2 of double
//
//let n = Num(8.9)
//let n2 = Num(8.9)
//n+n2

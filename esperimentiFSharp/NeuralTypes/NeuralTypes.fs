module NeuralTypes

open System
open System.Collections.Generic

type ActivationFunType = seq<double * double> -> double   // input * peso
type OutputFunType = double -> double

// Funzioni di uscita
let sigmoid (t : double) : double = 1.0 / (1.0 + exp(-t))
let heavside (t :double) (theta :double) : double =    if t < theta then
                                                                    0.0
                                                               else
                                                                    1.0
let linear (t : double) : double = t                    // utile per predire valori numerici

// Funzioni di attivazione
let sumOfProducts (input : seq<double * double> ) : double = input 
                                                                |> Seq.fold (fun acc el -> match el with (a,b) -> acc + a*b) 0.0 

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

// In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

// - Applicare la memoizzazione sulle funzioni come Mean, ...
// - Usare un DataSet per rappresentare le tabelle (forse conviene)
// - Usare le sequence (con using) per leggere il file

let random = new System.Random()

type ActivationFunType = (double * double) list -> double   // input * peso
type OutputFunType = double -> double

let sigmoid (t : double) : double = 1.0 / (1.0 + exp(-t))
let sumOfProducts (input : (double * double) list) : double = input 
                                                                |> List.fold (fun acc el -> match el with (a,b) -> acc + a*b) 0.0 

type Neuron(nInput : int, actFun : ActivationFunType, outFun : OutputFunType) =
    
    let random = new System.Random()
    let weights = Array.init nInput (fun _ -> random.NextDouble())

    member n.ActivationFunction = actFun
    member n.OutputFunction = outFun
    member n.Weights = weights

    member n.Activate(inputs : double []) =
        if inputs.Length <> weights.Length then
            (invalidArg "inputs" "Incompatible number of inputs for the neuron" : unit)
        [ for i in 0 .. (inputs.Length-1) -> (inputs.[i], weights.[i]) ] 
            |> actFun
            |> outFun 

let neuron1 = new Neuron(3, sumOfProducts, sigmoid)
neuron1.Activate([| for i in 0 .. 2 -> random.NextDouble() |])

    




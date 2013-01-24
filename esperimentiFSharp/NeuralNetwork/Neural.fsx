
// In un modulo a parte si possono definire le varie funzioni di base (somma, sigmoid, gradino, ...)

// - Applicare la memoizzazione sulle funzioni come Mean, ...
// - Usare un DataSet per rappresentare le tabelle (forse conviene)
// - Usare le sequence (con using) per leggere il file

let random = new System.Random()

type ActivationFunType = (double * double) list -> double   // input * peso
type OutputFunType = double -> double


// Funzioni di uscita
let sigmoid (t : double) : double = 1.0 / (1.0 + exp(-t))
let heavisied (t :double) (theta :double) : double =    if t < theta then
                                                            0.0
                                                        else
                                                            1.0

// Funzioni di attivazione
let sumOfProducts (input : (double * double) list) : double = input 
                                                                |> List.fold (fun acc el -> match el with (a,b) -> acc + a*b) 0.0 

type Neuron(nInput : int, actFun : ActivationFunType, outFun : OutputFunType) =
    
    let weights = Array.init nInput (fun _ -> random.NextDouble())

    member n.ActivationFunction = actFun
    member n.OutputFunction = outFun
    member n.Weights = weights

    member n.Activate(inputs : double list) =
        if inputs.Length <> weights.Length then
            (invalidArg "inputs" "Incompatible number of inputs for the neuron" : unit)
        [ for i in 0 .. (inputs.Length-1) -> (inputs.[i], weights.[i]) ] 
            |> actFun
            |> outFun 

let layer1 = [ for i in 0 .. 9 -> new Neuron(3, sumOfProducts, sigmoid) ]
let layer2 = [ for i in 0 .. 4 -> new Neuron(10, sumOfProducts, sigmoid) ]
let layer3 = new Neuron(5, sumOfProducts, sigmoid)
let inputs = [ 0.5; 0.000099; 0.06 ];;

let outputsLayer1 = layer1 
                        |> List.map (fun n -> n.Activate(inputs))

let outputsLayer2 = layer2
                        |> List.map (fun n -> n.Activate(outputsLayer1))

let outputLayer3 = layer3.Activate(outputsLayer2)

    




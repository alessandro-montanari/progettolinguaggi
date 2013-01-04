// Problem 1 ----------------------------------
// If we list all the natural numbers below 10 that are multiples of 3 or 5, we get 3, 5, 6 and 9. The sum of these multiples is 23.
// Find the sum of all the multiples of 3 or 5 below 1000.

let naturals = seq { 0 .. 999 }

// Alternativa 1
let mutipleOfThree n =
    n % 3 = 0
let mutipleOfFive n =
    n % 5 = 0
let mutipleOfThreeOrFive n =
    mutipleOfThree n || mutipleOfFive n
let multiplesOfThreeOrFive sequence =
    sequence |> Seq.filter mutipleOfThreeOrFive
let sumOfMultiplesOF3And5 sequence =
    sequence |> multiplesOfThreeOrFive |> Seq.sum
let sum4 = seq { 0 .. 999 } |> sumOfMultiplesOF3And5

// Alternativa 2
let sum = seq { 0 .. 999 } |> Seq.filter (fun el -> el % 3 = 0 || el % 5 = 0 ) |> Seq.sum

// Alternativa 3
let sum2 = seq { for i in 0 .. 999 do if i % 3 = 0 || i % 5 = 0 then yield i } |> Seq.sum

// Alternativa 4
let rec sumOfMultiplesOF3And5Rec list =
    match list with
    | [] -> 0
    | h :: t when h % 3 = 0 || h % 5 = 0 -> h + sumOfMultiplesOF3And5Rec t
    | h :: t -> sumOfMultiplesOF3And5Rec t
let sum3 = sumOfMultiplesOF3And5Rec [ 0 .. 999 ]

// Per capire l'ondemand delle sequences
[| 0I .. 99999999I |] |> Array.filter (fun el -> el % 3I = 0I || el % 5I = 0I ) |> Array.sum        // Arriva in breve a 2 GB di memoria senza calcolare il risultato (System.OutOfMemoryException: Out of memory)
seq { 0I .. 99999999I } |> Seq.filter (fun el -> el % 3I = 0I || el % 5I = 0I ) |> Seq.sum          // Con 72 MB di memoria arriva al risultato


// Problem 2 ----------------------------------

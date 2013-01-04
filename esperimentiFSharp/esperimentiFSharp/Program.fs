
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.
#light

module esperimentiFSharp.Main

//---- Cap 2 ----

open System
open System.Windows.Forms
open System.Drawing

let printSomething =
    printfn "Something!!"

// I nomi utilizzati devono essere dei parametri del costruttore o delle propriettà dell'oggetto   
let form = new Form(Text="Welcome")    
                                
form.Visible <- true

do Application.Run(form)     

                                              
                                                                                            
//----- Network -----
open System.IO
open System.Net

let http url =
    let req = WebRequest.Create(url:string)
    let resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    let reader = new StreamReader(stream)
    let html = reader.ReadToEnd()
    html
http "http://www.google.com"

//---- Cap 3 ----

// OPERATORI ARITMETICI                                    
//    - non sono "checked", ossia non lanciano eccezione in caso di overflow o underflow
//    - è possibile estendere il significato degli operatori classici a tipi di dato custom
//    - una divisione per zero tra numeri in floating point genera un risultato di tipo Infinity o -Infinity
//    - se non diversamete specificato il sistema di type inference assume che gli operatori siano utilizzati tra interi, per modificare ciò è necessario specificare 
//    il tipo di alemeno un parametro

let getInfinity a:float = a / 0.0


// STRINGHE (immutabili)

let classicString = "A string"
// Utile per i percorsi dei file
let verbatimString = @"c:\Program Files"
classicString.[3]
classicString.[2..7]
let concatenateString = "First" + " " + "Second" + " " + "Third"

 
// LISTE (immutabili)

let emptyList = []
let valueList = [2; 4; 6]
let consList = 2 :: emptyList
let consList2 = 56 :: consList
let concatList = consList @ consList2
let rangeIntegers = [1 .. 99]
let rangeFloat = [1.0 .. 99.0]
let generatedList = [ for x in 1 .. 99 -> x*x]

// pattern matching
let printHead list =
    match list with
    | h :: t -> printfn "The head is %d" h
    | [] -> printfn "The list is empty"

List.length valueList

let initFunction i:int = i*4
List.init 5 initFunction
List.map initFunction valueList
List.map (fun el -> el*el) valueList
        
let isEven element = element % 2 = 0
List.filter isEven rangeIntegers
List.filter (fun el -> el % 2 = 0) rangeIntegers

// OPTION VALUES
// Rappresenta un valore (Some) o (OPPURE) assenza di valore (None)
// Definizione di Option
//  type 'T option =
//      | None
//      | Some of 'T

let people = [ ("Adam", None);
               ("Eve", None);
               ("Cain", Some("Adam", "Eve"))]
               
let showParents (name, parents) = 
    match parents with
    | Some(dad,mom) -> printfn "%s has father %s and mother %s" name dad mom
    | None -> printfn "%s has no parents!!" name
showParents ("Cain", Some("Adam", "Eve"))

// Gli Option values vengono spesso utilizzati per rappresentare il successo o il fallimento di una computazione
let fetch url =
    try Some (http url)
    with :? System.Net.WebException -> None
match (fetch "http://www.microsoft.com") with
| Some html -> printfn "text = %s" html
| None -> printfn "No page found"

// FUNZIONI RICORSIVE
//  let rec ...
// A default le funzioni non sono ricorsive, occorre utilizzare la parola chiave rec per abilitarla.
// Questo per rendere esplicite le funzioni che sono ricorsive per migliorare la manutenibilità del codice.
let rec factorial n =
    if n <= 1 then 1
    else n * factorial (n-1)            // Qui le parentesi ci vogliono, altrimenti va in stack overflow
factorial 7

let rec factorialBigInt (n:bigint) : bigint =
    if n <= bigint 1 then bigint 1
    else n * factorialBigInt (n-bigint 1)
factorialBigInt (bigint 7000)

// NB: per chiamare una funzione che non prende parametri occorre specificare il valore unit (void) tramite due parentesi tonde "()"
// altrimenti, solo il nome della funzione rappresenta la funzione stessa la quale però non viene invocata


// FUNCTION VALUES

let sites = [ "http://www.live.com"; "http://www.microsoft.com" ]
let fetch url = (url, http url)
List.map fetch sites                    // Si noti come si passa la funzione "fetch" ad un'altra funzione (List.map viene anche chiamata aggregate operators)
                                        // Alcuni tipi di dati dispongono anche del metodo Map che ha lo stesso significato di List.map ad esempio ( sitest.Map (fun ...) )

// Anonymous function values
let primeCubes = List.map (fun x -> x*x*x) [2; 3; 5; 7]     // Qui la funzione è anonima e appare come una espressione piuttosto che tramite let

let resultOfFetch = List.map (fun url -> (url, http url)) sites
let lenghtOfSites = List.map (fun (_,html) -> String.length html) resultOfFetch     // Pattern matching per utilizzare la tupla + wildcard per ignorare il primo elemento della tupla

let delimiters = [| ' '; '\n'; '\t'; '<'; '>'; '=' |]
let getWords (text:string) = text.Split delimiters
let getStats site =
    let url = "http://" + site
    let html = http url
    let nWords = html |> getWords                                       // Operatore pipeline, aiuta la leggibilità e il sistema di type inference
    let nRef = nWords |> Array.filter (fun el -> el.Equals "href")      // Si poteva anche scrivere fun el -> el = "href" (= non è l'assegnamento in questo caso)
    (site, html.Length, nWords.Length, nRef.Length)
List.map getStats [ "www.google.com"; "www.microsoft.com"; "www.facebook.com" ]

// Composizione di funzioni (operatore >>)
// Permette di comporre diverse funzioni per ottenere così una nuova funzione in un modo più leggibile
let countLinks = getWords >> Array.filter (fun el -> el = "href") >> Array.length       // Non occorre indicare che countLinks prende un parametro in ingresso 
                                                                                        // perché viene inferito da getWords
// Partial apllication
// E' possibile creare nuove funzioni fissando (bind) uno o più parametri di una funzione già esistente
let add x y = x + y
let add10 = add 10

let addTuples (x, y) (t, z) = (x+t, y+z)
let addTuples10 = addTuples (10, 10)

open System
let measureTime f =
    let start = DateTime.Now
    let res = f ()
    let finish = DateTime.Now
    (res, finish-start)
measureTime (fun () -> http "http://www.google.com")


// PATTERN MATCHING
// Il controllo dei pattern avviene dall'alto verso il basso e appena si ha un match si valuta l'espressione alla destra di "->"
// Il compilatore si accorge dei match mancanti e di quelli ridondanti (warning)

let urlFilter url agent =
    match (url, agent) with         // Tupla
    | "http://www.google.com", 99 -> true
    | "http://www.microsoft.com", _ -> false
    | _, 86 -> true
    | _ -> false

// E' possibile utilizzare delle condizioni come guadia in caso di matching (keyword when)
let sing x =
    match x with
    | _ when x < 0 -> -1
    | _ when x > 0 -> 1
    | _ -> 0
    
let checkHead list =
    match list with
    | 3 :: _ -> printfn "The first element is a 3"
    | n :: _ when n < 0 -> printfn "The first element is negative"
    | _ :: _ | [] -> failwith "incorrect first element"

let checkTail list =
    match list with
    | _ :: t when t = [ 4; 5; 6 ] -> printfn "OK!"
    | [] | _ :: _ -> failwith "error"


// SEQUENCES
// Una struttura che è iterabile (tipo System.Collections.Generic.IEnumerable<type> in .NET)
// Le sequences sono calcolate on demand (lazy) nel senso che i valori sono calcolati e restituiti solo se necessario, solo se si accede a quei valori

seq {0 .. 2}
seq {1I .. 10000000000000000I}
let intSeq = seq {1 .. 2 .. 100}

// Iteration
for i in intSeq do
    printfn "i = %d" i
intSeq |> Seq.iter (fun el -> printfn "i = %d" el)

Seq.delay (fun () -> seq {1.0 .. 1000.0} ) |> Seq.iter (fun el -> printfn "i = %e" el)

open System.IO
let rec allFiles dir =
    Seq.append
        (dir |> Directory.GetFiles)
        (dir |> Directory.GetDirectories |> Seq.map allFiles |> Seq.concat)     // Seq.map applica la funzione on demand, il che vuoldire che le sottodirectory non sono lette fino a che non è strettamente necessario
allFiles @"/dev" |> Seq.length

// Sequence Expressions

// for
let squares = seq { for i in 0 .. 10 -> (i, i*i) }                      // Modo più compatto di scrivere quello sotto
let squares2 = seq { 0 .. 10 } |> Seq.map (fun el -> (el, el*el) )

// Dopo la keyword "for" ci può essere un pattern
// Dopo la keyword "in" ci può essere una sequence o qualsiasi cosa che supporta GetEnumerator
seq { for (i, square) in squares -> (i, square, i*square) }

// Altri usi delle Sequence Expressions
let coordinates n =
    seq { for row in 1 .. n do
            for col in 1 .. n do
                if(row+col) % 2 = 0 then
                    yield (row,col) }

let rec allFiles2 dir =
    seq { for file in Directory.GetFiles dir do
            yield file
          for subdir in Directory.GetDirectories dir do
            yield! allFiles2 dir }                          // il "!" indica che l'ultimo (e solo l'ultimo può essere così) yeild genera una sequenza invece che un singolo valore
            












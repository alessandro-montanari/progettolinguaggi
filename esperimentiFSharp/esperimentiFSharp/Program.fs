
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
            
// Tutti i modi visti per costruire le sequences possono essere utilizzati anche per costruire liste o array utilizzando rispettivamente [ ] e [| |]
let array = [| for i in 1 .. 10 -> (i, i*i) |]

// Funzione Seq.fold
// Applica una funzione ad ogni elemento della sequenza portandosi avanti (elemento dopo elemento) lo stato della computazione,
// infatti la funzione prende in ingresso uno "lo stato corrente" e un elemento della sequenza e calcola il nuovo stato
let sumSeq sequence1 = Seq.fold (fun acc elem -> acc + elem) 0 sequence1
Seq.init 10 (fun index -> index * index)
|> sumSeq
|> printfn "The sum of the elements is %d."

// TYPE DEFINITIONS

// Type Abbreviations
// Sono "espansi" ai tipi sottostanti durante la compilazione, in pratica da un'altro linguaggio .NET non si vedono le abbreviazioni
type index = int
let i:index = 6

type StringMap<'T> = Microsoft.FSharp.Collections.Map<string, 'T>

// Records (sono estendibili)
// Di solito utilizzati per restituire valori dalle funzioni quando devono essere restituiti molti valori diversi

type Person =                               // Definizione
    { Name: string
      DateOfBirth: System.DateTime; }
let ale = { Name = "Ale"; DateOfBirth = new System.DateTime(1988, 6, 6) }                 // Uso
( { Name = "Ale"; DateOfBirth = new System.DateTime(1988, 6, 6) } : Person )              // (in caso di abiguità nei nomi dei campi)
ale.DateOfBirth

let aleClone = { ale with Name = "AleClone" }

// Discriminated Unions (non sono estendibili, cioè le alternative definite sono fisse)

type Route = int
type Make = string
type Model = string
type Transport =
    | Car of Make*Model
    | Bicycle
    | Bus of Route
    
let car1 = Car ("BMW", "360") 
let averageSpeed trasp =
    match trasp with
    | Car _ -> 50
    | Bicycle -> 10
    | Bus _ -> 35
averageSpeed car1

type Proposition =
    | True
    | And of Proposition*Proposition
    | Or of Proposition*Proposition
    | Not of Proposition
let rec eval (p:Proposition) =
    match p with
    | True -> true
    | And (p1, p2) -> eval p1 && eval p2
    | Or (p1, p2) -> eval p1 || eval p2    
    | Not p1 -> not (eval p1)



//---- Cap 4 ---- IMPERATIVE PROGRAMMING

// IMPERATIVE LOOPING
//  - for var = start-expr to end-expr do expr      (estremi inclusi)
//  - for pattern in expr do expr                   (per uso in sequences)
//  - while expr do expr
// Tutti e tre con tipi di ritorno dell'espressione del body = unit


// MUTABLE RECORDS
// Se uno o più campi di un record sono marcati come mutable
type MutableRecord =
    {   mutable Total : int;
        mutable State : bool;
        Name : string
    }
let record = { Total = 54; State = true; Name = "test" }
record.Total <- 100

// MUTABLE REFERENCE CELLS
// Molto simili ai puntatori del C ma zenza l'aritmetica
// I valori vengono memorizzati nell'heap e sono utilizzati nelle chiusure (vedi sotto)

// Creazione: "ref"
let valore = ref 5

// Lettura: "!" oppure ".contents" oppure ".Value"
!valore
valore.contents
valore.Value

// Modifica: ":=" 
valore := !valore + 2


// HIDING MUTABLE DATA
// Nella programmazione funzionale si cerca di incapsulare i dati variabili all'interno di confini ben definiti (funzioni e chiusure -> ambiente lessicale)

// CHIUSURA:    Nei linguaggi di programmazione, una chiusura è una astrazione che combina una funzione con le variabili libere presenti 
//              nell'ambiente in cui è definita secondo le regole di scope del linguaggio. Le variabili libere dell'ambiente rimangono accessibili per 
//              tutta la durata di vita (extent) della chiusura e pertanto persistono nel corso di invocazioni successive della chiusura. Di conseguenza, 
//              le variabili della chiusura possono essere usate per mantenere uno stato ed emulare costrutti della programmazione a oggetti.
//              Le chiusure si rivelano utili quando una funzione ha bisogno di "ricordare" informazioni: ad esempio un parametro specifico per un'operazione 
//              di confronto, oppure il riferimento ad un widget in un callback di un'interfaccia grafica.

// Le variabili locali in F# (e in generale "in teoria") non possono essere catturate in chiusure, infatti se si utilizza una variabile locale "mutable" al posto di ref
// il compilatore segnala il seguente errore: " ... Mutable variables cannot be captured by closures. Consider eliminating this use of mutation or using a heap-allocated 
// mutable reference cell via 'ref' and '!'. "
// Infatti le variabili locali essendo allocate nello stack vengono distrutte dopo la chiamata alla funzione, mentre le variabili catturate in una chiusura devono 
// sopravvivere e quindi di solito vengono allocate nell'heap. 
// 
let generateStamp =
    let count = ref 0                               // Questa riga viene eseguta solo UNA volta all'atto della definizione di "generateStamp"
    ( fun () -> count := !count + 1; !count ) 

// In questo caso il valore x viene catturato non appena "f" viene definita e dato che x non può cambiare la funzione f utilizzerà sempre il valore 2.
// NB: anche se si modifica il valore di x rieseguendo l'esperssione "let x = 6" la funzione "f" continua ad utilizzare il "vecchio" valore di x che 
// è proprio quello catturato nella chiusura, se invece si utilizza una reference cell il valore restituito da "f" cambia se si cambia il valore di x
let x = 2
let f num = num + x 

let y = ref 4
let g num = num + !y
g 5
y := !y + 2
g 5


// ARRAY
// Gli array sono mutable e sono zero-indexed
// NB: array di "value types" sono allocati in modo "piatto" ossia viene allocato un solo oggetto. 
// In .NET sono value types tutti i tipi primitivi e altri (chiamati di solito struct) 
// IN F# è possibile definire dei value types (vedi capitolo 6) ma tutti gli altri tipi (esclusi i primitivi)
// sono dei reference type, come ad esempio i record, le tuple, i discriminated unions e le classi

let arr = [| 1; 2; 3 |]
arr.[1]
arr.[1] <- 5
arr

// Possono essere costruiti con la sintassi delle sequence expressions
let arr2 = [| 1 .. 100 |]
let arr3 = [| for i in 1 .. 100 do 
                if i % 2 = 0 then
                    yield i |]
let arr4 = [| for i in 1 .. 100 -> (i*i) |]

// Nel modulo Array ci sono diverse funzioni utili e gli Aggregate Operators
arr4 |> Array.iter (fun el -> printfn "%d" el)
arr4 |> Array.map (fun el -> (el, el*el) )
let revArr4 = Array.rev arr4

// Slice Notation
// In questi casi vengono generati nuovi array
arr4.[3..9]
arr4.[3..]
arr4.[..5]

// Array bidimensionali
let arr2D = Array2D.init 5 5 (fun i j -> i*j)
arr2D.[0,0]
arr2D.[0,0] <- 56

// Non esiste una sintassi per scrivere array literal multidimensionali è necessario usare il modulo Array2D o l'operatore array2D
let my2DArray = array2D [ [ 1; 0]; [0; 1] ]     // Lista di liste con gli elementi dell'array

// E' possibile utilizzare tutte le collezioni presenti nel framework .NET
// In alcuni casi sono state definite delle abbreviazioni in F# come ad esempio per le liste: type ResizeArray<'T> = System.Collections.Generic.List<'T>
open System.Collections.Generic

let netList = new List<string>()
netList.Add("ciao")
netList.Add("ciao2")
netList.Add("ciao3")
netList.[0]

// NB: i resizeble array utilizzano un array nella implementazione e garantiscono un tempo di accesso random costante, quindi in molti casi
// hanno delle performance più elevate delle liste di F# che supportano un'accesso efficiente solo per la testa della lista

// NB2: queste liste supportano l'interfaccia seq<'T> quindi possono essere utilizzate come le sequence
let squaresRes = new ResizeArray<int>(seq { for i in 0 .. 100 -> i*i })
squaresRes |> Seq.iter (fun el -> printfn "%d" el )

// Dizionari
let capitals = new Dictionary<string, string>()
capitals.["USA"] <- "Washington"
capitals.["Bangladesh"] <- "Dhaka"

// I dizionari sono compatibili con il tipo seq<KeyValuePair<'key, 'value>> dove KeyValuePair non è altro che un tipo che ha le due proprietà
// Key e Value, quindi anche in questo caso possono essere utilizzate le funzione del modulo Seq
capitals |> Seq.iter ( fun el -> printfn "%s = %s" el.Key el.Value )

// Dato che il metodo Dictionary.TryGetValue utilizza un "out" parameter in F# può essere usato in tre modi diversi
// 1 - usando un valore mutable e l'operatore & (indirizzo-di)
let lookupName nm (dict : Dictionary<string, string>) =
    let mutable res = ""
    let foundIt = dict.TryGetValue(nm, &res)
    if foundIt then res
    else failwithf "Didn't find %s" nm

// 2 - usando una reference cell
let res = ref ""
capitals.TryGetValue("Australia", res)
capitals.TryGetValue("USA", res)

// 3 - se non si passa l'ultimo parametro il risultato viene restituito come una tupla (bool * 'T)
capitals.TryGetValue("Australia")
capitals.TryGetValue("USA")

// Per rappresentare delle mappe sparse può essere conveniente utilizzare delle chiavi composte per un dizionario, come ad esempio una tupla
let sparseMap = new Dictionary<(int * int), float>()
sparseMap.[(0,2)] <- 4.0


// ECCEZIONI

// Lancio
(raise (System.InvalidOperationException("not today")) : unit)
(failwith "error" : unit)
(invalidArg "x" "y" : unit)

// Cattura
// NB: "try .. with .. " è una singola espressione e quindi è possibile restituire una valore in entrambi i rami
open System.IO
open System.Net

let http url =
    try
        let req = WebRequest.Create(url:string)
        let resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        html
    with
        | :? System.UriFormatException -> ""
        | :? System.Net.WebException -> ""
        | exn -> ""                         // Cattura qualsiasi eccezione. "exn" è un'abbreviazione per System.Exception
http "aaa"

// L'operatore :? può essere usato anche con il costrutto "match ... with"
let switchOnType (a:obj) =          // "obj" abbreviazione per System.Object
    match a with
        | null                      -> printf "null"
        | :? System.Exception as e  -> printf "Eccezione %s" e.Message
        | :? System.Int32 as i      -> printf "Intero %d" i
        | _                         -> printf "Altro"

// Finally
let httpViaTryFinally url =
    let req = WebRequest.Create(url:string)
    let resp = req.GetResponse()
    try
        let stream = resp.GetResponseStream()
        let reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        html
    finally
        resp.Close()

// In F# non esiste il costrutto "try .. with .. finally .."
// anche perché di solito per pulire le risorse si utilizza il binding "use" che chiude le risorse 
// alla fine dello scope di quel binding
let httpViaUseBinding url =
    let req = WebRequest.Create(url:string)
    use resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    let reader = new StreamReader(stream)
    let html = reader.ReadToEnd()
    html

// Definire delle nuove eccezioni
exception BlockedURL of string
try
    raise(BlockedURL("..."))
with
    | BlockedURL(url) -> printfn "Bloccato! url = %s" url               // Pattern matching in "| BlockedURL(url)"


// BASIC I/O

// Files
// Si utilizzano principalmente le classi di .NET System.IO.File e System.IO.Directory
open System.IO

File.WriteAllLines("test.txt", [| "line 1"; "line 2" |])
File.ReadAllLines("test.txt")
File.Delete("test.txt")

// Stream
let outStream = File.CreateText("test.txt")
outStream.WriteLine("Line a")
outStream.WriteLine("Line b")
outStream.Close()

let inStream = File.OpenText("test.txt")
inStream.ReadLine()
inStream.Close()
 
// Nel seguente modulo ci sono diverse funzioni di formattazione 
//  Microsoft.FSharp.Core.Printf

// Codici di formattazione più comuni
//  - &b : boolean
//  - %s : string
//  - %d : int
//  - %e : float
//  - %A : Any Type
//  - %O : Usa Object.ToString()
//  - %a : prende due argomenti, uno è la funzione di formattazione e uno è il valore dal formattare
//  - %t : esegue la funzione passata come parametro


// Side effects e lazy computation
// Programmare in uno stile imperativo e voler utilizzare la valutazione lazy (ad esempio restituendo una funzione che esegue la computazione)
// potrebbe portare a degli errori come nell'esempio successivo:
open System.IO 
let readr1, reader2 =
    let reader = new StreamReader(File.OpenRead("test.txt"))
    let firstReader() = reader.ReadLine()
    let secondReader() = reader.ReadLine()
    reader.Close()                              //Chiudo lo stream ma restituisco le due funzioni che lo usano
    firstReader, secondReader

// Una soluzione potrebbe essere quella di utilizzare la keyword use all'interno di una sequence expression, così che lo stream sia chiuso automaticamente (vedi cap 8)
let reader =
    seq {   use reader = new StreamReader(File.OpenRead("test.txt"))
            while not reader.EndOfStream do
                yield reader.ReadLine() }


//---- Cap 5 ---- MASTERING TYPES AND GENERICS

// Generic Comparison
// Gli operatori classici di confronto (=, <, >, ...) sono generici e possono funzionare su diversi tipi di dato, in particolare sulla maggiorparte dei tipi strutturati
// di F#, come liste e tuple
("abc", "def") < ("abc", "xyz")
compare (10, 30) (10, 20)
compare [10; 30] [10; 20]

// Generic Hashing
// Restituisce un intero che rappresenta l'hash stabile del valore in input
hash "abc"
hash (100, "abc")

// Realizzare funzioni generiche
// Si consideri il codice per il massimo comune divisore
let rec hcf a b =
    if a=0 then b
    elif a<b then hcf a (b-a)
    else hcf (a-b) b
// L'algoritmo non è generico perché lavora solo su interi (int -> int -> int). Anche se l'operatore (-) è overloaded in F#, ogni uso dell'operatore
// DEVE essere associato al più ad un solo tipo deciso a compile time (int a default). In più, la costante zero è un intero e non è overloaded.
// Per rendere generico il codice è necessario fornire ESPLICITAMENTE lo zero, una funzione di sottrazione e una di ordinamento.
let hcfGeneric (zero, sub, lessThan) =
    let rec hcf a b =
        if a=zero then b                                // "when 'a : equality" : il tipo generico 'a deve supportare il confronto
        elif lessThan a b then hcf a (sub b a)
        else hcf (sub a b) b
    hcf
    
let hcfInt = hcfGeneric (0, (-), (<))                   // Durante l'instanziazione della funzione generica si associano gli operatori ad un particolare tipo
let hcfInt64 = hcfGeneric (0L, (-), (<))

// Di solito si raggruppano i tre parametri della funzione genrica in un singolo tipo, un record o un'interfaccia
type Numeric<'T> =                              // Tipi come questo sono di solito chiamati "dictionaries of operations"
    {   Zero : 'T;
        Subtract : ('T -> 'T -> 'T);
        LessThan : ('T -> 'T -> bool); }

let intOps = { Zero=0; Subtract=(-); LessThan=(<) }
let int64Ops = { Zero=0L; Subtract=(-); LessThan=(<) }

let hcfGeneric (ops : Numeric<'T>) =
    let rec hcf a b =
        if a=ops.Zero then b                                // "when 'T : equality" : il tipo generico 'T deve supportare il confronto
        elif ops.LessThan a b then hcf a (ops.Subtract b a)
        else hcf (ops.Subtract a b) b
    hcf

let hcfInt = hcfGeneric intOps

// NB:  con questa tecnica è possibile rendere il codice generico utilizzabile con qualsiasi tipo di dato anche se tali tipi non sono correlati fra loro (in gerarchia)
//      Infatti è sufficiente passare il giusto insieme di operazioni (sub e lessThan) e valori (zero) per ottenere una funzione che opera sul tipo voluto.
//      In questo modo quindi si rende ESPLICITA la fattorizzazione delle funzioni, ossia per ottenere una funzione per il mio tipo devo espressamente passare le funzioni 
//      richieste. Nella programmazione ad oggetti invece si tende a fattorizzare gli algortimi in un modo IMPLICITO e basato sulla gerarchia dei tipi, quindi non è necessario
//      passare nessuna funzione o dato aggiuntivo ma si perde in flessibilità in quanto quegli algoritmi possono essere utilizzati solo in base alle relazioni tra i tipi
//      decisi dal programmatore.



//---- Cap 6 ---- WORKING WITH OBJECTS AND MODULES (OOP)


// Aggiungere membri ad un record
type Vector2D =
    {   DX : float;
        DY : float }
    // Property
    member v.Lenght = sqrt(v.DX * v.DX + v.DY * v.DY)       // L'identificatore "v" sta per "this". In F# si può usare qualsiasi identificatore valido per denotare "this".
    
    // Method
    member v.Scale(k) = { DX=k*v.DX; DY=k*v.DY }
    
    // Static property
    static member Zero = { DX=0.0; DY=0.0 }

let vector2D = { DX=3.0; DY=5.0 }
vector2D.Lenght
let vectorScaled = vector2D.Scale(3.0)

// NB: dato che nel caso dei record e delle union i valori utilizzati per costruire l'oggetto sono gli stessi che sono mmorizzati nell'oggetto stesso, il compilatore è 
// in grado di inferire automaticamente le funzioni di confronto, uguaglianza ed hashing.

//Aggiungere membri ad una discriminated union
type Tree<'T> =
    | Node of 'T * Tree<'T> * Tree<'T>
    | Tip
    
    member t.Size =
        match t with
        | Node(_,l,r) -> 1+ l.Size + r.Size
        | Tip -> 0
        
// Come scegliere se usare le funzioni o i membri?
// Di solito si utilizzano i membri per le proprietà e le operazioni che sono strettamente correlate con il tipo che si sta definendo, mentre si utilizzano funzioni 
// statiche agginte ad un modulo per le funzionalità addizionali

// CLASSI
type Vector2DClass(dx : float, dy : float) =     // dx e dy sono visibili per tutti i membri non statici della definizione. Qui stiamo anche definendo un construttore implicito 
                                                 // con due argomenti di tipo float, in realtà è una tupla di due float (vedi definizione del tipo)
    let len = sqrt(dx * dx + dy * dy)       // l'espressione dopo l'= viene calcolata ad ogni costruzione di un oggetto. len è come se fosse un field privato della classe
    
    // Accessor properties
    member v.DX = dx
    member v.DY = dy
    member v.Length = len
    
let v = new Vector2DClass(5.0, 4.0)
let v2 = Vector2DClass(6.0, 3.0)

// E' possibile cotrollare che i parametri d'ingresso al costruttore implicito rispettino certe condizioni tramite la parola chiave "do"
// E' possibbile definire altri costruttori con la parola chiave "new"
type UnitVector2D(dx, dy) =
    let tolerance = 0.000001
    let length = sqrt(dx*dy + dy*dy)
    do if abs (length - 1.0) >= tolerance then failwith "not a unit vector";

    new() = UnitVector2D(1.0, 0.0)

    member v.DX = dx
    member v.DY = dy

 // Property in get e set
 type MyType() =
    let mutable x = ""
    let mutable y = 1
    member t.X  with get() = x
                and set v = x <- v
    member t.Y  with get() = y
                and set v = y <- v 

let myType = MyType(X="ciao", Y=54)
myType.X <- "ciao ciao"

// INTERFACCE

open System.Drawing
type IShape =
    abstract Contains : Point -> bool
    abstract BoundingBox : Rectangle

// Funzione che crea un cerchio dato il centro e il raggio (object expression)
let circle (center : Point, radius : int) =
    { new IShape with
        member x.Contains(p : Point) =
            let dx = float32 (p.X - center.X)
            let dy = float32 (p.Y - center.Y)
            sqrt(dx*dx+dy*dy) <= float32 radius

        member x.BoundingBox =
            Rectangle(center.X-radius, center.Y-radius, 2*radius+1,2*radius+1) }

// Implementazione con un tipo concreto
type MutableCircle() =
    let mutable center = Point(x=0, y=0)
    let mutable radius = 10

    interface IShape with
        member x.Contains(p : Point) =
            let dx = float32 (p.X - center.X)
            let dy = float32 (p.Y - center.Y)
            sqrt(dx*dx+dy*dy) <= float32 radius

        member x.BoundingBox =
            Rectangle(center.X-radius, center.Y-radius, 2*radius+1,2*radius+1)

let mutCircle = MutableCircle()
(mutCircle :> IShape).BoundingBox

// MODULI
// Contenitore di tipi, funzioni, stato globale e sottomoduli
// Sono compilati in classi che contengono solo valori statici, tipi e sottomoduli

type Vector2D =
    { DX : float; DY : float }

module Vector2DOps =
    let length v = sqrt( v.DX*v.DX + v.DY*v.DY )
  
// E' possibile aprire un modulo per non dover specificare il suo nome quando si utilizzano i suoi contenuti




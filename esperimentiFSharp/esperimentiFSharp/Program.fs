
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

let req = WebRequest.Create("http://www.microsoft.com")
let resp = req.GetResponse()
let stream = resp.GetResponseStream()
let reader = new StreamReader(stream)
let html = reader.ReadToEnd()
printfn "%s" html

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
    
let isEven element = element % 2 = 0
List.filter isEven rangeIntegers
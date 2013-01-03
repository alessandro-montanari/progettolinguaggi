
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.
#light

module esperimentiFSharp.Main

open System
open System.Windows.Forms
open System.Drawing

let printSomething =
    printfn "Something!!"

// I nomi utilizzati devono essere dei parametri del costruttore o delle propriettà dell'oggetto   

let form = new Form(Text="Welcome")                                          
form.FormBorderStyle <- FormBorderStyle.Sizable
form.Width <- 300
form.Height <- 300

do Application.Run(form)
                                                        
form.Text <- "Testo modificato"

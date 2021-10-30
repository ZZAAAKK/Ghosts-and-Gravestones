open System

open UserInterface
open System.Windows.Forms

[<STAThread>]
[<EntryPoint>]
let main argv =
    let UI = new UserInterface()
    Application.Run(UI.Form)
    0
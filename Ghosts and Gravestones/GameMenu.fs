module GameMenu

open System
open System.Windows.Forms

type GameMenu (_callback : EventArgs -> unit) =
    let newGame = new MenuItem("New Game...", fun _ -> _callback)
    let exit = new MenuItem("Exit")
    let file = new MenuItem("File", [| newGame; exit |])
    let howToPlay = new MenuItem("How to play")
    let about = new MenuItem("About")
    let help = new MenuItem("Help", [| howToPlay; about |])
    let menu = new MainMenu([| file; help |])
    do
        exit.Click.Add(fun _ -> Application.Exit())
        howToPlay.Click.Add(fun _ -> MessageBox.Show("There are only a few simple rules to this puzzle game:
        1) Your job is to place ghosts next to gravestones.
        2) Each ghost is attached to only one gravestone.
        3) The numbers outside the grid tell you how many ghosts are 
            in the respective row or column.
        4) A ghost can only be found horizontally or vertically 
            adjacent to a gravestone.
        5) Ghosts are never adjacent to each other, neither vertically, 
            horizontally, nor diagonally.
        6) A gravestone may be next to two ghosts, but is only 
            connected to one." , "How to play") |> ignore)
        about.Click.Add(fun _ -> MessageBox.Show("This game was created for the Oct 2021 F# game jam on itch.io by Swift.\n\nFor more info, go to https://s-w-i-f-t.itch.io/", "About") |> ignore)
    member this.Menu = menu
module UserInterface

open System.Drawing
open System.Windows.Forms
open Microsoft.VisualBasic

open Board
open BoardArgs
open GameMenu

type UserInterface () =
    let form = new Form()
    let mutable board = new Board(new BoardArgs(new Size(5, 5)))
    
    let CreateNewBoard n = 
        board <- new Board(new BoardArgs(new Size(n, n)))
        form.Controls.Clear()
        form.Controls.Add(board.Panel)
        form.ClientSize <- board.Panel.Size

    let BindInput n =
        match n with
        | x when x < 4 -> 4
        | x when x > 15 -> 15
        | _ -> n

    let CreateNewGame () =
        try
            Interaction.InputBox("Enter board width between 4 and 15\nN.B. All boards are square.", "Create New Game", "5") 
            |> int 
            |> BindInput
            |> CreateNewBoard
        with
            | _ -> 5 |> CreateNewBoard

    do
        form.Icon <- LocalResourceManager.GetIconResource("Ghost1")
        form.Text <- "Ghosts and Gravestones"
        form.FormBorderStyle <- FormBorderStyle.None
        form.Location <- new Point(0, 0)
        form.Controls.Add(board.Panel)
        form.Menu <- (new GameMenu(fun _ -> CreateNewGame())).Menu
        form.ClientSize <- board.Panel.Size

    member this.Form = form   
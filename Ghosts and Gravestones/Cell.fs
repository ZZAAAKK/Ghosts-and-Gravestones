module Cell

open System.Windows.Forms
open System.Drawing

open LocalResourceManager
open CellState
open CellArgs

[<AllowNullLiteral>]
type Cell (args : CellArgs) =
    let label = new Label()
    let mutable cellState = if args.IsClue then Clue elif args.IsGraveStone then Gravestone else Empty
    let mutable value = args.Value
    let textVisible = args.TextVisible
    let row = args.Row
    let column = args.Column
    let SetBackgroundImage() =
        match cellState with
        | Ghost | Gravestone -> GetImageResource($"{cellState}")
        | _ -> null
    let ToggleCellState() =
        match cellState with
        | Empty -> 
            label.BackColor <- Color.FromArgb(0, (new System.Random()).Next(80, 180), 0)
            cellState <- Blank
        | Blank -> 
            label.BackgroundImage <- GetImageResource("Ghost")
            cellState <- Ghost
        | Ghost -> 
            label.BackgroundImage <- null
            label.BackColor <- Color.FromArgb(55, 55, 55)
            cellState <- Empty
        | _ -> ()
    let Rollback() =
        match cellState with
        | Empty -> 
            label.BackgroundImage <- GetImageResource("Ghost")
            cellState <- Ghost
        | Blank -> 
            label.BackColor <- Color.FromArgb(55, 55, 55)
            cellState <- Empty
        | Ghost -> 
            label.BackgroundImage <- null
            cellState <- Blank
        | _ -> ()
    do
        label.Location <- args.ScaledLocation
        label.AutoSize <- false
        label.Size <- args.ScaledSize
        label.Text <- if args.TextVisible && not args.IsTopLeftCorner then args.Value.ToString() else null
        label.TextAlign <- ContentAlignment.MiddleCenter
        label.Font <- new Font("Segoe UI", 24f)
        label.BackgroundImageLayout <- ImageLayout.Stretch
        label.BackgroundImage <- if args.IsTopLeftCorner then GetImageResource("Undo") else SetBackgroundImage()
        label.BorderStyle <- if args.TextVisible then BorderStyle.None else BorderStyle.FixedSingle
        label.BackColor <- if args.TextVisible then Color.Gainsboro elif args.IsGraveStone then Color.FromArgb(0, (new System.Random()).Next(80, 180), 0) else Color.FromArgb(55, 55, 55)
        label.Click.Add(fun _ -> ToggleCellState())
        label.Click.Add(args.Callback)
        label.Click.Add(fun _ -> args.BreadcrumbCallback(args.Row, args.Column))

    member this.Label = label
    member this.Row = args.Row
    member this.Column = args.Column
    member this.State = cellState
    member this.Value with get() = value and set(v) = value <- v
    member this.IsGhost = args.IsGhost
    member this.RollBack() = Rollback()
    member this.UpdateClue(n) =
        value <- n
        label.Text <- $"{value}"
    member this.Greenify() = label.BackColor <- Color.FromArgb(0, (new System.Random()).Next(80, 180), 0)
    member this.Ghostify() = 
        cellState <- Ghost
        label.BackgroundImage <- SetBackgroundImage()
        this.Greenify()
    member this.Gravify() =
        cellState <- Gravestone
        label.BackgroundImage <- SetBackgroundImage()
        this.Greenify()
    member this.HideGhost() =
        cellState <- Empty
        label.BackgroundImage <- SetBackgroundImage()
        label.BackColor <- Color.FromArgb(55, 55, 55)
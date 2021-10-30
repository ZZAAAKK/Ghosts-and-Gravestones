module Board

open System
open System.Windows.Forms
open System.Drawing
open System.Linq

open Cell
open CellArgs
open CellState
open BoardArgs
open Breadcrumb

type Board (args : BoardArgs) =
    let panel = new Panel()
    let mutable cells : Cell[] = [||]
    let mutable firstRow : Cell[] = [||]
    let mutable firstColumn : Cell[] = [||]
    let mutable breadcrumbs : Breadcrumb list = []
    let ghostCount = Math.Floor(((args.Width * args.Height) |> float) / (5 |> float)) |> int
    
    let GhostCells () =
        query {
            for c in cells do
            where (c.State = Ghost)
            select c
            }
    
    let GravestoneCells () =
        query {
            for c in cells do
            where (c.State = Gravestone)
            select c
            }

    let CheckVictory () =
        let mutable isVictory = true
        for c in firstRow do
            let n = query {
                for cell in cells do
                where (cell.Column = c.Column && cell.State = Ghost)
                count}
            isVictory <- n = c.Value
        for c in firstColumn do
            let n = query {
                for cell in cells do
                where (cell.Row = c.Row && cell.State = Ghost)
                count}
            isVictory <- n = c.Value
        isVictory <- GhostCells().Count() = ghostCount
        if isVictory then
            panel.Enabled <- false
            MessageBox.Show("You win!\n\nSelect 'New Game' from the menu to play again.", "Game over", MessageBoxButtons.OK) |> ignore

    let CheckCells () =
        for c in firstRow do
            let n = query {
                for cell in cells do
                where (cell.Column = c.Column && cell.State = Ghost)
                count}
            c.Label.ForeColor <- if n = c.Value then Color.DarkGray elif n > c.Value then Color.Red else Color.Black
        for c in firstColumn do
            let n = query {
                for cell in cells do
                where (cell.Row = c.Row && cell.State = Ghost)
                count}
            c.Label.ForeColor <- if n = c.Value then Color.DarkGray elif n > c.Value then Color.Red else Color.Black
        CheckVictory()

    let GetCellByAddress(c : Breadcrumb) =
        query {
            for cell in cells do
            where (cell.Row = c.Row && cell.Column = c.Column)
            exactlyOneOrDefault
            }
            
    let CreateBreadcrumb (_row, _col) =
        breadcrumbs <- new Breadcrumb(_row, _col) :: breadcrumbs

    let RollbackCell(cell : Cell) =
        if not (cell |> isNull) then
            cell.RollBack()
            CheckCells()

    let RollbackSingleMove () =
        if breadcrumbs.Length > 0 then
            breadcrumbs.[0] |> GetCellByAddress |> RollbackCell
            breadcrumbs <- match breadcrumbs with
                            | h::t -> t
                            | _ -> []

    let GetSurroundingCells (c : Cell) =
        [ for i in [-1..1] do
            for j in [-1..1] do
                if not (i = 0 && j = 0) then
                    yield! cells.Where(fun x -> x.Row = (c.Row + i) && x.Column = (c.Column + j)).Take(1) ]
                    
    let GetSurroundingOrthogonalCells (c : Cell) =
        [ for i in [-1;1] do
                yield! cells.Where(fun x -> x.Row = (c.Row + i) && x.Column = c.Column).Take(1)
                yield! cells.Where(fun x -> x.Row = c.Row && x.Column = (c.Column + i)).Take(1)]

    let FillRemainingEmptyCellsInRow (_row, _col) =
        let n = query {
            for cell in cells do
            where (cell.Column = _col && cell.State = Ghost)
            count}
        if n = firstRow.Where(fun x -> x.Column = _col).First().Value then
            for cell in (query {
                for cell in cells do
                where (cell.State = Empty && cell.Column = _col)
                select cell
                }) do cell.Greenify()

    let FillRemainingEmptyCellsInColumn (_row, _col) =
        let n = query {
            for cell in cells do
            where (cell.Row = _row && cell.State = Ghost)
            count}
        if n = firstColumn.Where(fun x -> x.Row = _row).First().Value then
            for cell in (query {
                for cell in cells do
                where (cell.State = Empty && cell.Row = _row)
                select cell
                }) do cell.Greenify()

    let PlaceGhosts () =
        let mutable unavailableCells = []
        let mutable consecutiveFails = 0
        let mutable failedToFindSolution = false
        firstRow <- [| for i in [1..args.Width] do yield new Cell(new CellArgs(0, i, args.Scale, 0, false, (fun _ -> CheckCells()), FillRemainingEmptyCellsInRow)) |]
        firstColumn <- [| for i in [1..args.Height] do yield new Cell(new CellArgs(i, 0, args.Scale, 0, false, (fun _ -> CheckCells()), FillRemainingEmptyCellsInColumn)) |]
        cells <- [| for i in [1..args.Width] do
                        for j in [1..args.Height] do
                            yield new Cell(new CellArgs(i, j, args.Scale, 0, false, (fun _ -> CheckCells()), CreateBreadcrumb)) |]
        let rand = new Random()
        while not failedToFindSolution do
            let rec GetRandomCell () =
                let index = rand.Next(0, cells.Length)
                if not (GhostCells() |> Seq.contains(cells.[index])) && not (unavailableCells |> List.contains(cells.[index])) then
                    let surroundingCells = GetSurroundingCells(cells.[index])
                    if not ((Enumerable.Intersect(GetSurroundingCells(cells.[index]), GhostCells())) |> Enumerable.Any) then
                        cells.[index].Ghostify()
                        unavailableCells <- cells.[index] :: unavailableCells
                        consecutiveFails <- 0
                        for c in surroundingCells do 
                            unavailableCells <- c :: unavailableCells
                consecutiveFails <- consecutiveFails + 1
                if consecutiveFails > 2 * (args.Width * args.Height) then failedToFindSolution <- true else GetRandomCell()
            if consecutiveFails > 2 * (args.Width * args.Height) then 
                failedToFindSolution <- true 
            else 
                consecutiveFails <- consecutiveFails + 1
                GetRandomCell()

    let CountGhosts () =
        for c in firstRow do
            let n = query {
                for cell in cells do
                where (cell.Column = c.Column && cell.State = Ghost)
                count}
            c.UpdateClue(n)
        for c in firstColumn do
            let n = query {
                for cell in cells do
                where (cell.Row = c.Row && cell.State = Ghost)
                count}
            c.UpdateClue(n)

    let PlaceGravestones () =
        let mutable unavailableCells = []
        let rand = new Random()
        for cell in GhostCells() do
            let surroundingCells = cell |> GetSurroundingOrthogonalCells |> List.except(unavailableCells)
            let index = rand.Next(0, surroundingCells.Length)
            if not (GravestoneCells() |> Seq.contains(surroundingCells.[index])) then
                surroundingCells.[index].Gravify()
                unavailableCells <- surroundingCells.[index] :: unavailableCells
                
    let RemoveTemporaryGhosts () =
        for cell in GhostCells() do
            cell.HideGhost()

    do
        while not (GhostCells().Count() = ghostCount) do PlaceGhosts()
        CountGhosts()
        PlaceGravestones()
        RemoveTemporaryGhosts()
        CheckCells()
        panel.Controls.Add((new Cell(new CellArgs(0, 0, args.Scale, 0, false, (fun _ -> RollbackSingleMove()), CreateBreadcrumb))).Label)
        panel.Controls.AddRange([| for c in firstRow do yield c.Label |])
        panel.Controls.AddRange([| for c in firstColumn do yield c.Label |])
        panel.Controls.AddRange([| for c in cells do yield c.Label |])
        panel.Size <- args.Size
    
    member this.Panel = panel
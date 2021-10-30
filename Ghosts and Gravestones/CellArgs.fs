module CellArgs

open System
open System.Drawing

type CellArgs (_row, _col, _scale, _value : int, _isGravestone : bool, _callback : EventArgs -> unit, _breadcrumbCallback : int * int -> unit) =
    member this.Row = _row
    member this.Column = _col
    member this.IsClue = _row = 0 || _col = 0
    member this.Value = _value
    member this.TextVisible = _row = 0 || _col = 0
    member this.IsGraveStone = _isGravestone
    member this.IsGhost = _value = 1 && _row > 0 && _col > 0
    member this.IsTopLeftCorner = _row = 0 && _col = 0
    member this.Callback = _callback
    member this.BreadcrumbCallback = _breadcrumbCallback
    member this.Scale = _scale
    member private this.ScaledSideLength = 64f * this.Scale
    member this.ScaledLocation = new Point(int (float32 _col * this.ScaledSideLength), int (float32 _row * this.ScaledSideLength))
    member this.ScaledSize = new Size(int this.ScaledSideLength, int this.ScaledSideLength)
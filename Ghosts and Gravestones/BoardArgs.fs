module BoardArgs

open System.Drawing

type BoardArgs (_size : Size) =
    member this.Width = _size.Width
    member this.Height = _size.Height
    member private this.FloatWidth = float32 this.Width + 1f
    member private this.FloatHeight = float32 this.Height + 1f
    member this.Scale = 10f / float32 _size.Width
    member this.Size = new Size(int ((64f * this.Scale) * this.FloatWidth), int ((64f * this.Scale) * this.FloatHeight))
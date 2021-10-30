module LocalResourceManager

open System.Drawing
open System.Resources
open System.Reflection

let GetImageResource(resourceName : string) : Image =
    ResourceManager("Ghosts_and_Gravestones.Resources", Assembly.GetCallingAssembly()).GetObject(resourceName) :?> Image

let GetIconResource(resourceName : string) : Icon =
    ResourceManager("Ghosts_and_Gravestones.Resources", Assembly.GetCallingAssembly()).GetObject(resourceName) :?> Icon
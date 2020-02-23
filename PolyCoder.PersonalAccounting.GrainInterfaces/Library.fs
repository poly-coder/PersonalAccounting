namespace PolyCoder.PersonalAccounting.GrainInterfaces

open System.Threading.Tasks
open Orleans

type IHello = 
    inherit IGrainWithIntegerKey

    abstract member SayHello : string -> Task<string>
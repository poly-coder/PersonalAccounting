namespace PolyCoder.PersonalAccounting.Grains

open Orleans
open PolyCoder.PersonalAccounting.GrainInterfaces
open System.Threading.Tasks

type HelloGrain () =
  inherit Grain ()

  interface IHello with 
      member this.SayHello (name : string) : Task<string> = 
          name
              |> sprintf "Hello %s back!"
              |> Task.FromResult
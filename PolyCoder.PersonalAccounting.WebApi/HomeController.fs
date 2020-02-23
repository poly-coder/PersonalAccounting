namespace PolyCoder.PersonalAccounting.WebApi

open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks.V2
open Orleans
open PolyCoder.PersonalAccounting.GrainInterfaces

[<ApiController>]
[<Route("api/home")>]
type HomeController
    (
        grainFactory: IGrainFactory
    ) =
    inherit ControllerBase()

    let getHello() =
        grainFactory.GetGrain<IHello>(0L)

    [<HttpGet>]
    member this.Index([<FromQuery>] name: string) = task {
        let! response = getHello().SayHello(name)

        return this.Ok(response)
    }

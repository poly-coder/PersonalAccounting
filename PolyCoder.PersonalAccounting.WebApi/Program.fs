namespace PolyCoder.PersonalAccounting.WebApi

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Hosting
open Orleans.Configuration
open System.Net
open PolyCoder.PersonalAccounting.GrainInterfaces
open PolyCoder.PersonalAccounting.Grains
open Orleans.ApplicationParts

module Program =
    let exitCode = 0

    let useLocalhostClustering (siloBuilder: ISiloBuilder) =
        siloBuilder.UseLocalhostClustering()

    let configureAppParts partConfigurator (siloBuilder: ISiloBuilder) =
        siloBuilder.ConfigureApplicationParts(Action<_> (partConfigurator >> ignore))

    let addAppPart<'t> (parts: IApplicationPartManager) =
        parts.AddApplicationPart(typeof<'t>.Assembly)

    let withCodeGeneration (parts: IApplicationPartManagerWithAssemblies) =
        parts.WithCodeGeneration()

    let withClusterOptions clusterId serviceId (siloBuilder: ISiloBuilder) =
        siloBuilder.Configure(fun (opts: ClusterOptions) ->
            opts.ClusterId <- clusterId
            opts.ServiceId <- serviceId
        )

    let withAdvertisedIPAddress ipAddress (siloBuilder: ISiloBuilder) =
        siloBuilder.Configure(fun (opts: EndpointOptions) ->
            opts.AdvertisedIPAddress <- ipAddress
        )

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )
            .UseOrleans(fun siloBuilder ->
                siloBuilder
                    |> useLocalhostClustering
                    |> configureAppParts (fun parts ->
                        parts
                            |> addAppPart<IHello>
                            |> addAppPart<HelloGrain>
                            |> withCodeGeneration
                    )
                    |> withClusterOptions "dev" "WebApi"
                    |> withAdvertisedIPAddress IPAddress.Loopback
                |> ignore
            )
            

    [<EntryPoint>]
    let main args =
        CreateHostBuilder(args).Build().Run()

        exitCode

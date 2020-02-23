namespace PolyCoder.PersonalAccounting.WebApi

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services
            |> fun services -> services.AddControllers()
            |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            |> fun app ->
                if env.IsDevelopment() then app.UseDeveloperExceptionPage()
                else app
            |> fun app -> app.UseHttpsRedirection()
            |> fun app -> app.UseRouting()
            |> fun app -> app.UseAuthorization()
            |> fun app -> app.UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore)
            |> ignore


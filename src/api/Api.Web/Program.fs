module api

open Microsoft.Extensions.DependencyInjection

open Api.Web.Services
open Falco.HostBuilder

let startWebHost args =
    webHost args{
        endpoints Routes.all
        add_service (fun services -> services.AddScoped<EnvironmentService, EnvironmentService>())
    }
    
[<EntryPoint>]
let main argv =
    startWebHost argv
    0
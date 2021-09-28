module api

open Microsoft.Extensions.DependencyInjection

open Api.Web.Services
open Falco.HostBuilder

let startWebHost args =
    
    let environmentService = new EnvironmentService()
    
    webHost args{
        endpoints Routes.all
        add_service (fun services ->  services.AddSingleton(environmentService))
        add_service (fun services ->  services.AddScoped<ServiceStack.Messaging.IMessageService>(fun provider ->
            (MessageQueueService.configure environmentService) :> ServiceStack.Messaging.IMessageService
        ))
    }
    
[<EntryPoint>]
let main argv =
    startWebHost argv
    0
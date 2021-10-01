module api

open Api.Domain.Stores
open Microsoft.Extensions.DependencyInjection

open Api.Web.Services
open Falco.HostBuilder

let startWebHost (args : string[]) = 
    let config = Api.Config.Parsing.fromFile (args.[0])
    
    let services = {
        AppStore = (InMemoryApplicationStore() :> IApplicationStore)
        Configuration = config;
        MessageQueue = (MessageQueueService.configure config)
    }
    
    webHost args {
        endpoints (Routes.all services)
    }
    
[<EntryPoint>]
let main argv =
    startWebHost argv
    0
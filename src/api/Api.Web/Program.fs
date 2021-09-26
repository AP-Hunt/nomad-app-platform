module api

open Falco.HostBuilder

[<EntryPoint>]
let main argv =
    webHost argv {
        endpoints Routes.all
    }
    
    0
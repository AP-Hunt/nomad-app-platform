module api

open Falco.HostBuilder

let startWebHost args =
    webHost args{
        endpoints Routes.all
    }
    
[<EntryPoint>]
let main argv =
    startWebHost argv
    0
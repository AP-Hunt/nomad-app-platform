module api

open System
open System.Threading
open Suave

[<EntryPoint>]
let main argv =
    let cts = new CancellationTokenSource()
    let conf = { defaultConfig with cancellationToken = cts.Token }
    
    let app =
        choose [
            AppRoutes.routes
        ]
    
    let _, server = startWebServerAsync conf app
    
    Async.Start(server, cts.Token)
    printfn "Server has started"
    Console.ReadKey true |> ignore
    cts.Cancel()
    
    0
open System
open System.Threading
open Suave

[<EntryPoint>]
let main argv =
    let cts = new CancellationTokenSource()
    let conf = { defaultConfig with cancellationToken = cts.Token }
    let _, server = startWebServerAsync conf (Successful.OK "Hello World")
    
    Async.Start(server, cts.Token)
    printfn "Server has started"
    Console.ReadKey true |> ignore
    cts.Cancel()
    
    0
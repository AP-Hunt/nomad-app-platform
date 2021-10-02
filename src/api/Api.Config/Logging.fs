namespace Api.Config.Logging

open System
open Serilog

type Logger(config : Api.Config.Configuration) =
    let mutable _logger = null
    
    let maybeLogContext (context : 'T option) (logger : Serilog.ILogger) = 
        match context with
        | Some(c) -> logger.ForContext("context", c, true)
        | None -> logger
        
    do
        _logger <- (new LoggerConfiguration())
            .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), config.Logging.LogPath)
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .Enrich.FromLogContext()
            .Destructure.FSharpTypes()
            .CreateLogger()
    
    member this.Verbose (message : string, ?context : obj) =
        _logger.ForContext("context", context, true).Verbose(message)
        (_logger |> maybeLogContext context).Verbose(message)
        
    
    member this.Debug (message : string, ?context : obj) =
        (_logger |> maybeLogContext context).Debug(message)
        
    member this.Info (message : string, ?context : obj) =
        (_logger |> maybeLogContext context).Information(message)
        
    member this.Warn (message : string, ?context : obj) =
        (_logger |> maybeLogContext context).Warning(message)
        
    member this.Error (message : string, errorMessage : string) =
        let context = Some {| Error = errorMessage |}
        (_logger |> maybeLogContext context).Error(message)
        
    member this.Error (message : string, exc : Exception) =
        let context = Some {| Exception = exc |}
        (_logger |> maybeLogContext context).Error(message)
        
    member this.Error (message : string, ?context : obj) =
        (_logger |> maybeLogContext context).Error(message)        
        
    member this.Fatal (message : string, ?context : obj) =
        (_logger |> maybeLogContext context).Fatal(message)                                      
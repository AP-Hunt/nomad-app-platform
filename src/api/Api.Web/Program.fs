module api

open Api.Domain.Stores
open Api.Web.Services

open Serilog
open Serilog.Sinks.SystemConsole
open Serilog.Sinks.File
open Falco.HostBuilder

let createLogger (config : Api.Config.Configuration) =
    (new LoggerConfiguration())
        .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), config.Logging.LogPath)
        .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
        .Enrich.FromLogContext()
        .Destructure.FSharpTypes()
        .CreateLogger()

let startWebHost (args : string[]) = 
    let config = Api.Config.Parsing.fromFile (args.[0])
    let logger = new Api.Config.Logging.Logger(config)
    
    let dbContext = new Api.Domain.Persistence.Context(config.Database.ConnectionString)
    dbContext.ApplyMigrations()
    
    let services = {
        AppStore = (Api.Domain.Persistence.ApplicationStore(dbContext) :> IApplicationStore)
        Configuration = config
        Logger = logger;
        MessageQueue = (MessageQueueService.configure config)
    }
    
    logger.Info("start-web-server")
    webHost args {
        endpoints (Routes.all services)
    }
    
[<EntryPoint>]
let main argv =
    startWebHost argv
    0
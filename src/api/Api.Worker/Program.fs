module Worker

open System

open Api.Config
open Api.Domain.Stores
open Api.Worker
open ServiceStack.Messaging.Redis
open ServiceStack.Redis

[<EntryPoint>]
let main argv =
    let config = Api.Config.Parsing.fromFile argv.[0]
    Storage.ensureStoragePaths config
    
    let logger = new Api.Config.Logging.Logger(config)
    
    let redisFactory = new PooledRedisClientManager(config.MessageQueue.RedisAddress) 
    let messageQueueServer = new RedisMqServer(redisFactory)
    messageQueueServer.RetryCount <- config.MessageQueue.RetryCount
    
    let dbContext = new Api.Domain.Persistence.Context(config.Database.ConnectionString)
    dbContext.ApplyMigrations()
    let appStore = (Api.Domain.Persistence.ApplicationStore(dbContext) :> IApplicationStore)
    
    messageQueueServer.RegisterHandler<Api.Domain.Messages.DeployAppMessage>(
        DeployApplication.deployApplicationHandler logger config appStore
    )
    
    messageQueueServer.Start()
    
    logger.Info("worker-started")
    Console.WriteLine("Press Enter to stop")
    Console.ReadLine() |> ignore
    
    0 // return an integer exit code
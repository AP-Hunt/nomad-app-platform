module Worker

open System

open ServiceStack.Messaging.Redis
open ServiceStack.Redis

[<EntryPoint>]
let main argv =
    let config = Api.Config.Parsing.fromFile argv.[0]
    let logger = new Api.Config.Logging.Logger(config)
    
    let redisFactory = new PooledRedisClientManager(config.MessageQueue.RedisAddress)
    
    let messageQueueServer = new RedisMqServer(redisFactory)
    messageQueueServer.RetryCount <- config.MessageQueue.RetryCount
    
    messageQueueServer.RegisterHandler<Api.Domain.Messages.DeployAppMessage>(Handlers.deployApplicationHandler logger)
    
    messageQueueServer.Start()
    
    logger.Info("worker-started")
    Console.WriteLine("Press Enter to stop")
    Console.ReadLine() |> ignore
    
    0 // return an integer exit code
module Worker

open System
open ServiceStack.Messaging
open ServiceStack.Messaging.Redis
open ServiceStack.Redis

[<EntryPoint>]
let main argv =
    let config = Api.Config.Parsing.fromFile argv.[0]
    let redisFactory = new PooledRedisClientManager(config.MessageQueue.RedisAddress)
    
    let messageQueueServer = new RedisMqServer(redisFactory)
    messageQueueServer.RetryCount <- config.MessageQueue.RetryCount
    
    messageQueueServer.RegisterHandler<Api.Domain.Messages.DeployAppMessage>(Handlers.deployApplicationHandler)
    
    messageQueueServer.Start()
    
    Console.WriteLine("Worker has begun listening.")
    Console.WriteLine("Press Enter to stop")
    Console.ReadLine() |> ignore
    
    0 // return an integer exit code
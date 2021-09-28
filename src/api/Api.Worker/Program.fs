module Worker

open System
open ServiceStack.Messaging
open ServiceStack.Messaging.Redis
open ServiceStack.Redis

[<EntryPoint>]
let main argv =
    let redisFactory = new PooledRedisClientManager(Environment.GetEnvironmentVariable("MESSAGE_QUEUE_REDIS_ADDRESS"))
    
    let messageQueueServer = new RedisMqServer(redisFactory)
    messageQueueServer.RetryCount <- 2
    
    messageQueueServer.RegisterHandler<Api.Domain.Messages.DeployAppMessage>(Handlers.deployApplicationHandler)
    
    messageQueueServer.Start()
    
    Console.WriteLine("Worker has begun listening.")
    Console.WriteLine("Press Enter to stop")
    Console.ReadLine() |> ignore
    
    0 // return an integer exit code
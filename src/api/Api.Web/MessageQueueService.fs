module MessageQueueService

open Api.Web.Services
open ServiceStack.Redis
open ServiceStack.Messaging.Redis

let configure (environment : EnvironmentService) =
    let redisFactory = new PooledRedisClientManager(environment.MessageQueueRedisAddress)
    let messageQueue = new RedisMqServer(redisFactory)
    messageQueue.RetryCount <- 2
    messageQueue


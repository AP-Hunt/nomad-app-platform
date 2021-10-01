module MessageQueueService

open ServiceStack.Redis
open ServiceStack.Messaging.Redis

let configure (config : Api.Config.Configuration) =
    let redisFactory = new PooledRedisClientManager(config.MessageQueue.RedisAddress)
    let messageQueue = new RedisMqServer(redisFactory)
    messageQueue.RetryCount <- config.MessageQueue.RetryCount
    messageQueue


namespace Api.Config

open System.Text.Json


[<CLIMutable>]
type BlobStoreConfiguration = {
    SourceBundleStoragePath: string
}

[<CLIMutable>]
type DockerRegistryConfiguration = {
    RegistryAddress: string
}


[<CLIMutable>]
type LoggingConfiguration = {
    LogPath: string
}


[<CLIMutable>]
type MessageQueueConfiguration = {
    RedisAddress: string
    RetryCount: int
}


[<CLIMutable>]
type Configuration =
    {
        BlobStore: BlobStoreConfiguration
        DockerRegistry: DockerRegistryConfiguration
        Logging: LoggingConfiguration
        MessageQueue: MessageQueueConfiguration
        
    }

module Parsing =
    
    let fromFile filePath =
        let text = System.IO.File.ReadAllText filePath
        JsonSerializer.Deserialize<Configuration> text
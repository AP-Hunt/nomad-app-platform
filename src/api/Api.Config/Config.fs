namespace Api.Config

open System.Text.Json

[<CLIMutable>]
type DockerRegistryConfiguration = {
    RegistryAddress: string
}

[<CLIMutable>]
type MessageQueueConfiguration = {
    RedisAddress: string
    RetryCount: int
}

[<CLIMutable>]
type BlobStoreConfiguration = {
    SourceBundleStoragePath: string
}

[<CLIMutable>]
type Configuration =
    {
        DockerRegistry: DockerRegistryConfiguration
        MessageQueue: MessageQueueConfiguration
        BlobStore: BlobStoreConfiguration
    }

module Parsing =
    
    let fromFile filePath =
        let text = System.IO.File.ReadAllText filePath
        JsonSerializer.Deserialize<Configuration> text
namespace Api.Config

open System.Text.Json


[<CLIMutable>]
type BlobStoreConfiguration = {
    SourceBundleStoragePath: string
    TerraformStatePath: string
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
type NomadConfiguration = {
    ApiAddress: string
    
    // Docker registry details to be used
    // when deploying applications to Nomad
    DockerRegistry: DockerRegistryConfiguration
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
        Nomad: NomadConfiguration
    }

module Parsing =
    
    let fromFile filePath =
        let text = System.IO.File.ReadAllText filePath
        JsonSerializer.Deserialize<Configuration> text
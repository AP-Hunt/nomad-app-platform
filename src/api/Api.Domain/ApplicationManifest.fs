namespace Api.Domain.ApplicationManifest

open System
open Api.Domain.Applications

[<CLIMutable>]
type ApplicationManifest =
    {
        Name: string
    }
    
module ApplicationManifest =
    
    open YamlDotNet.Serialization
    
    let fromYaml (yaml : string) =
        let deserializer = (new DeserializerBuilder())
                               .WithNamingConvention(NamingConventions.UnderscoredNamingConvention.Instance)
                               .Build()
        
        try                               
            Some(deserializer.Deserialize<ApplicationManifest>(yaml))
        with
        | :? YamlDotNet.Core.YamlException -> None
        | :? Exception -> reraise()
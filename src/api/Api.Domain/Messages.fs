namespace Api.Domain.Messages

open System
open System.IO
open Api.Domain.Applications

[<CLIMutable>]
type DeployAppMessage =
    {
        AppId: Guid
        Version: int
        SourcePath: string
    }

module MessagePublishing =
    
    let deployApp (app : Application) (pathInBlobStore : string) =
        { AppId = app.Id; Version = app.Version; SourcePath = pathInBlobStore }
        
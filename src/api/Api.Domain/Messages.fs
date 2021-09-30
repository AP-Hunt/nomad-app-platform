namespace Api.Domain.Messages

open System.IO
open Api.Domain.Applications

[<CLIMutable>]
type DeployAppMessage =
    {
        AppId: string
        Version: int
        SourcePath: string
    }

module MessagePublishing =
    
    let deployApp (app : Application) (pathInBlobStore : string) =
        { AppId = app.Id.Value; Version = app.Version; SourcePath = pathInBlobStore }
        
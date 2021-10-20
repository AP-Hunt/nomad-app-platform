namespace Api.Domain.Messages

open System
open System.IO
open Api.Domain.Applications

[<CLIMutable>]
type DeployAppMessage =
    {
        AppId: string
        AppName: string
        Version: int
        SourcePath: string
    }

module MessagePublishing =
    
    let deployApp (app : Application) (pathInBlobStore : string) =
        { AppId = app.Id.ToString(); AppName = app.Name; Version = app.Version; SourcePath = pathInBlobStore }
        
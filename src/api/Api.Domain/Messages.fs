namespace Api.Domain.Messages

open Api.Domain.Applications

[<CLIMutable>]
type DeployAppMessage =
    {
        AppId: string
    }

module MessagePublishing =
    
    let deployApp (app : Application) = { AppId = app.Id.Value }
        
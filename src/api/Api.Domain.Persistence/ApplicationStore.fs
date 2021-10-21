namespace Api.Domain.Persistence

open Api.Domain.Applications
open EntityFrameworkCore.FSharp.DbContextHelpers

open Api.Domain.Stores

type ApplicationStore(context) =
    member val private _ctx : Context = context
    
    interface IApplicationStore with
        member this.All() = this._ctx.Apps |> List.ofSeq
        member this.FindApplicationByName(name) = tryFilterFirst <@ fun a -> a.Name = name @> this._ctx.Apps
        member this.Save(app) =
            let maybeExisting : Application option = tryFindEntity this._ctx app.Id
            
            match maybeExisting with
            | Some existing ->
                let appNextVer = existing |> Applications.incrementVersion
                
                appNextVer
                |> updateEntity this._ctx (fun a -> a.Id :> obj)
                |> ignore
                
                saveChanges this._ctx |> ignore
                appNextVer
            | None ->
                app |> addEntity this._ctx
                saveChanges this._ctx
                app

        member this.Get(id) (version) =
            query {
                for app in this._ctx.Apps do
                where (app.Id = id && app.Version = version)
                select app
            }
            |> tryFirstAsync
            |> Async.RunSynchronously
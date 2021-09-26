namespace Api.Domain.Stores

open Api.Domain.Applications

type public IApplicationStore =
    abstract FindApplicationByName: name: string -> Application option
    abstract Save: app : Application -> Application
    abstract All: unit -> Application list
    
type public InMemoryApplicationStore() =
    member val private _applications = List.empty<Application> with get, set
    
    interface IApplicationStore with
        member this.FindApplicationByName(name) =
            this._applications
            |> List.tryFind (fun a -> a.Name = name)
            
        member this.Save(app) =
            let savedApp = app |> Applications.generateId
            this._applications <- this._applications @ [savedApp]
            savedApp

        member this.All() = this._applications
            
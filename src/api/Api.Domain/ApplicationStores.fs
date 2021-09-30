namespace Api.Domain.Stores

open Api.Domain.Applications

type public IApplicationStore =
    abstract FindApplicationByName: name: string -> Application option
    abstract Save: app : Application -> Application
    abstract All: unit -> Application list
    
type public InMemoryApplicationStore() =
    let latest applications =
        applications
        |> List.sortByDescending (fun a -> a.Version)
        |> List.head

    member val private _applications = List.empty<Application> with get, set
     
    interface IApplicationStore with
        member this.FindApplicationByName(name) =
            let allByName =
                this._applications
                |> List.filter (fun a -> a.Name = name)
                
            match allByName with
            | [] -> None
            | ls -> ls |> latest |> Some
            
        member this.Save(app) =
            this._saveOrUpdate app this._save this._update

        member this.All() = this._applications
            
    member private this._saveOrUpdate app (save : Application -> Application) (update : Application -> Application) =
        let me = this :> IApplicationStore
        match me.FindApplicationByName(app.Name) with
        | None -> save app
        | Some a -> update a
        
    member this._save app =
        let savedApp = app |> Applications.generateId
        this._applications <- this._applications @ [savedApp]
        savedApp
        
    member this._update app =
        let savedApp = app |> Applications.incrementVersion
        this._applications <- this._applications @ [savedApp]
        savedApp
        
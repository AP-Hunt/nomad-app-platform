module AppRoutes
    open System.IO
    open Api.Web.Services
    open Microsoft.AspNetCore.Http
    
    open Falco
    open Falco.Routing
    
    open Api.Domain.Applications
    open Api.Domain.ApplicationManifest
    open Api.Domain.Stores
    open Api.Domain.SourceArchive
    open Api.Web.Pagination
    open Pagination
    
    let appStore = new InMemoryApplicationStore() :> IApplicationStore
    
    let apps_index_post : HttpHandler =
        fun ctx ->
            let formBinder (f : FormCollectionReader) : IFormFile option =
                f.TryGetFormFile "source_bundle"
            
            let extractAndConvertManifest archive =
                let manifestYaml = (archive |> SourceArchive.extractManifest).Value
                let appManifest = manifestYaml |> ApplicationManifest.fromYaml
                
                match appManifest with
                | Some(manifest) -> Ok(manifest)
                | None -> Error("manifest parsing error")
        
            let createApplication manifest =
                Ok {Application.Name = manifest.Name; Id = None}
            
            let saveApplication (app : Application) : Result<Application, string> =
                Ok(appStore.Save(app))
                
            let saveSourceBundle (bundle : IFormFile) blobStorePath (app : Application) =
                let bundlePath = sprintf "%s/%s.zip" blobStorePath app.Id.Value
                use bundleDestinationStream = File.OpenWrite(bundlePath)
                bundle.CopyTo(bundleDestinationStream)
                Ok(app)
                
            let uploadToTempStore (sourceBundle : IFormFile option) : HttpHandler =
                match sourceBundle with
                | Some (bundle) ->
                    let envService = ctx.GetService<EnvironmentService>()
                    match SourceArchive.validateArchive (bundle.OpenReadStream()) with
                    | Ok archive ->
                        let application =
                            Ok archive
                            |> Result.bind extractAndConvertManifest
                            |> Result.bind createApplication
                            |> Result.bind saveApplication
                            |> Result.bind (saveSourceBundle bundle envService.SourceBundleBlobStore)
                            
                        match application with
                        | Ok app -> 
                            Response.withStatusCode StatusCodes.Status202Accepted
                            >> Response.ofJson app
                        | Error err ->
                            Response.withStatusCode StatusCodes.Status400BadRequest
                            >> Response.ofJson {|Success = false; Error = err |}
                    | Error e ->
                        Response.withStatusCode StatusCodes.Status400BadRequest
                        >> Response.ofJson {|Success = false; Error = e.ToString() |}
                | None ->
                    Response.withStatusCode(StatusCodes.Status400BadRequest)
                    >> Response.ofJson {|Success = false; Error = "No archive uploaded"|} 
                
            Request.mapFormStream formBinder uploadToTempStore ctx
    
    let apps_index_get : HttpHandler =
        fun ctx ->
            Response.ofJson (appStore.All() |> paginatedCollectionOf) ctx
    
    let all = [
        get "/apps" apps_index_get
        post "/apps" apps_index_post
    ]
    
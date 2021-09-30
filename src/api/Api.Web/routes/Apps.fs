module AppRoutes
    open System
    open System.IO
    open Microsoft.AspNetCore.Http
    
    open Falco
    open Falco.Routing
    open ServiceStack.Messaging
    
    open Api.Domain.Applications
    open Api.Domain.ApplicationManifest
    open Api.Domain.Messages
    open Api.Domain.Stores
    open Api.Domain.SourceArchive
    open Api.Web.Services
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
                match appStore.FindApplicationByName(manifest.Name) with
                | Some(app) -> Ok app
                | None -> Ok {Application.Name = manifest.Name; Id = None; Version = 1}
            
            let saveApplication (app : Application) : Result<Application, string> =
                Ok(appStore.Save(app))
                
            let saveSourceBundle (bundle : IFormFile) blobStorePath (app : Application) =
                let bundlePath = $"%s{blobStorePath}/%s{app.Id.Value}.zip"
                use bundleDestinationStream = File.OpenWrite(bundlePath)
                bundle.CopyTo(bundleDestinationStream)
                Ok((app, bundlePath))
                
            let publishDeployAppMessage (messageService : IMessageService) (app, blobStorePath) =
                let message = MessagePublishing.deployApp app blobStorePath
                try
                    messageService.MessageFactory.CreateMessageProducer().Publish message
                    Ok(app)
                with
                | ex -> Error("failed to publish message: " + ex.Message)
                
            let errorResponse errMsg = 
                Response.withStatusCode StatusCodes.Status400BadRequest
                >> Response.ofJson {|Success = false; Error = errMsg |}
                
            let acceptAppUpload (sourceBundle : IFormFile option) : HttpHandler =
                match sourceBundle with
                | None -> errorResponse "No archive uploaded"
                | Some (bundle) ->
                    let envService = ctx.GetService<EnvironmentService>()
                    let messageService = ctx.GetService<IMessageService>()
                    match SourceArchive.validateArchive (bundle.OpenReadStream()) with
                    | Error e -> errorResponse (e.ToString())
                    | Ok archive ->
                        let application =
                            Ok archive
                            |> Result.bind extractAndConvertManifest
                            |> Result.bind createApplication
                            |> Result.bind saveApplication
                            |> Result.bind (saveSourceBundle bundle envService.SourceBundleBlobStore)
                            |> Result.bind (publishDeployAppMessage messageService) 
                            
                        match application with
                        | Error err -> errorResponse err
                        | Ok app -> 
                            Response.withStatusCode StatusCodes.Status202Accepted
                            >> Response.ofJson app
              
            Request.mapFormStream formBinder acceptAppUpload ctx
    
    let apps_index_get : HttpHandler =
        fun ctx ->
            Response.ofJson (appStore.All() |> paginatedCollectionOf) ctx
    
    let all = [
        get "/apps" apps_index_get
        post "/apps" apps_index_post
    ]
    
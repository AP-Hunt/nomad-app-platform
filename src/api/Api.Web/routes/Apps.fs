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
    
    let apps_index_post
        (logger : Api.Config.Logging.Logger)
        (appStore : IApplicationStore)
        (messageService : IMessageService)
        (blobStore : string)
        ctx =
        let formBinder (f : FormCollectionReader) : IFormFile option =
            f.TryGetFormFile "source_bundle"
        
        let extractAndConvertManifest archive =
            logger.Info("extract-manifest")
            let manifestYaml = (archive |> SourceArchive.extractManifest).Value
            let appManifest = manifestYaml |> ApplicationManifest.fromYaml
            
            match appManifest with
            | Some(manifest) -> Ok(manifest)
            | None ->
                logger.Error("invalid-manifest")
                Error("manifest parsing error")
    
        let createApplication manifest =
            match appStore.FindApplicationByName(manifest.Name) with
            | Some(app) -> Ok app
            | None -> Ok {Application.Name = manifest.Name; Id = Guid.NewGuid(); Version = 1}
        
        let saveApplication (app : Application) : Result<Application, string> =
            let savedApp = appStore.Save(app)
            logger.Info(
                "saved-application",
                {| Id = savedApp.Id; Version = savedApp.Version|}
            )
            Ok(savedApp)
            
        let saveSourceBundle (bundle : IFormFile) blobStorePath (app : Application) =
            let bundlePath = $"%s{blobStorePath}/%s{app.Id.ToString()}.zip"
            
            logger.Info("save-source-bundle", {| BundlePath = bundlePath |})
            use bundleDestinationStream = File.OpenWrite(bundlePath)
            bundle.CopyTo(bundleDestinationStream)
            Ok((app, bundlePath))
            
        let publishDeployAppMessage (messageService : IMessageService) (app, blobStorePath) =
            let message = MessagePublishing.deployApp app blobStorePath
            try
                messageService.MessageFactory.CreateMessageProducer().Publish message
                logger.Info("emit-deploy-message", {| Id = app.Id; Version = app.Version|})
                Ok(app)
            with
            | ex ->
                logger.Error("emit-deploy-message", ex)
                Error("failed to publish message: " + ex.Message)
            
        let errorResponse errMsg = 
            Response.withStatusCode StatusCodes.Status400BadRequest
            >> Response.ofJson {|Success = false; Error = errMsg |}
            
        let acceptAppUpload (sourceBundle : IFormFile option) : HttpHandler =
            match sourceBundle with
            | None ->
                logger.Error("no-archive-uploaded")
                errorResponse "No archive uploaded"
            | Some (bundle) ->
                logger.Info("accept-upload")
                match SourceArchive.validateArchive (bundle.OpenReadStream()) with
                | Error e -> errorResponse (e.ToString())
                | Ok archive ->
                    let application =
                        Ok archive
                        |> Result.bind extractAndConvertManifest
                        |> Result.bind createApplication
                        |> Result.bind saveApplication
                        |> Result.bind (saveSourceBundle bundle blobStore)
                        |> Result.bind (publishDeployAppMessage messageService) 
                        
                    match application with
                    | Error err ->
                        logger.Error("accept-upload", err)
                        errorResponse err
                    | Ok app -> 
                        Response.withStatusCode StatusCodes.Status202Accepted
                        >> Response.ofJson app
          
        Request.mapFormStream formBinder acceptAppUpload ctx
    
    let apps_index_get (appStore : IApplicationStore) ctx =
        Response.ofJson (appStore.All() |> paginatedCollectionOf) ctx

    let all services = [
        get "/apps" (apps_index_get services.AppStore)
        post "/apps" (apps_index_post
                          services.Logger
                          services.AppStore
                          services.MessageQueue
                          services.Configuration.BlobStore.SourceBundleStoragePath)
    ]
    
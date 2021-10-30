module Api.Worker.DeployApplication

open System
open System.IO
open Api.Domain.Applications
open Api.Domain.Deployment
open Handlers
open Api.Domain.Buildpacks
open Api.Domain.ContainerImages
open BuildpackExecutor

let createContainerImage (logger : Api.Config.Logging.Logger) (sourceBundlePath : string) (registry : string) (app : Application) = 
    logger.Info("create-container-image")
    let appImage =
        BuildpackExecutor.defaults
        |> buildpack "gcr.io/paketo-buildpacks/go" 
        |> registryAddress registry
        |> sourcePath sourceBundlePath
        |> (fun settings ->
            logger.Info(
                "create-container-image",
                {|
                    Id = app.Id;
                    Version = app.Version;
                    BuildpackSettings = settings;
                    SourcePath = sourcePath;
                |}
                )
            settings
        )
        |> run (app.Id.ToString()) (app.Version.ToString())
    
    match appImage with
    | Error(err) ->
        logger.Error("create-container-image", err)
        Error(err)
    | Ok(imageName) ->
        logger.Info("created-container-image")
        Ok(imageName)
        
let saveStateFile (config : Api.Config.Configuration) (appId : string) stateFilePath =
    File.Copy(stateFilePath, Path.Combine(config.BlobStore.TerraformStatePath, appId+".tfstate"))
    
let terraformDeploy
    (logger : Api.Config.Logging.Logger)
    (config : Api.Config.Configuration)
    (app : Application)
    (imageReference : string)
    =
    logger.Info("terraform-deploy")
    let containerImage = imageReference.Replace(config.DockerRegistry.RegistryAddress, config.Nomad.DockerRegistry.RegistryAddress)
    
    let tfVar name value = 
        logger.Info("set-variable", {|Name = name; Value = value|})
        Terraform.var name value
    
    logger.Info("prepare-terraform-working-dir")
    let prep =
        Terraform.prepareWorkingDirectory (Path.GetTempPath())
        |> Result.bind (fun wd ->
            logger.Info("terraform-working-dir", {|WorkingDir = wd|})
            Ok(wd)
        )
        |> Result.bind (tfVar "app_id" (app.Id.ToString()))
        |> Result.bind (tfVar "app_name" app.Name)
        |> Result.bind (tfVar "nomad_api" config.Nomad.ApiAddress)
        |> Result.bind (tfVar "container_image" containerImage)
       
    match prep with
    | Error err -> 
        logger.Error("prepare-terraform-working-dir", err)
        Error(err)
    | Ok(wd) ->
        logger.Info("terraform-apply")
        let applyResult = Terraform.apply wd
        
        match applyResult with
        | Error (wd, err) ->
            logger.Error("terraform-apply", err)
            saveStateFile config (app.Id.ToString()) (Terraform.stateFile wd)
            Error(err)
        | Ok(wd) ->
            logger.Info("terraform-applied")
            saveStateFile config (app.Id.ToString()) (Terraform.stateFile wd)
            Ok(wd)
            
let private deployApplication (logger : Api.Config.Logging.Logger) (config : Api.Config.Configuration) (appStore: Api.Domain.Stores.IApplicationStore) (message : Api.Domain.Messages.DeployAppMessage) : obj =
    logger.Info("deploy-application", {|AppId = message.AppId; Version = message.Version|})
    
    let maybeApplication = appStore.Get message.AppId message.Version
    
    match maybeApplication with
    | None ->
        logger.Error("app-not-found", {|AppId = message.AppId; Version = message.Version|})
    | Some app ->
        let result =
            app
            |> createContainerImage logger message.SourcePath config.Nomad.DockerRegistry.RegistryAddress
            |> Result.bind (terraformDeploy logger config app)
        
        match result with
        | Ok _ -> logger.Info("deployed-application", {|AppId = app.Id; Version = app.Version|})
        | Error err -> logger.Error("deploy-application", err)
    null

let deployApplicationHandler logger config appStore = asMessageHandler (deployApplication logger config appStore)
module Api.Worker.DeployApplication

open System.IO
open Api.Domain.Deployment
open Handlers
open Api.Domain.Buildpacks
open Api.Domain.ContainerImages
open BuildpackExecutor

let createContainerImage (logger : Api.Config.Logging.Logger) (message : Api.Domain.Messages.DeployAppMessage) = 
    logger.Info("create-container-image")
    let appImage =
        BuildpackExecutor.defaults
        |> buildpack "gcr.io/paketo-buildpacks/go" 
        |> registryAddress "localhost:6000"
        |> sourcePath message.SourcePath
        |> (fun settings ->
            logger.Info(
                "create-container-image",
                {|
                    Id = message.AppId;
                    Version = message.Version;
                    BuildpackSettings = settings;
                    SourcePath = sourcePath;
                |}
                )
            settings
        )
        |> run message.AppId (message.Version.ToString())
    
    match appImage with
    | Error(err) ->
        logger.Error("create-container-image", err)
        Error(err)
    | Ok(imageName) ->
        logger.Info("created-container-image")
        Ok(imageName)
        
let publishContainerImage (logger : Api.Config.Logging.Logger) appImage =
    logger.Info("publish-container-image")
    match ContainerImages.push(appImage) with
    | Ok(imageReference) ->
        logger.Info("published-container-image", {| Image = imageReference |})
        Ok(imageReference)
    | Error(err) ->
        logger.Error("publish-container-image", err)
        Error(err)
 
let saveStateFile (config : Api.Config.Configuration) (appId : string) stateFilePath =
    File.Copy(stateFilePath, Path.Combine(config.BlobStore.TerraformStatePath, appId+".tfstate"))
    
let terraformDeploy
    (logger : Api.Config.Logging.Logger)
    (config : Api.Config.Configuration)
    (message : Api.Domain.Messages.DeployAppMessage)
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
        |> Result.bind (tfVar "app_id" message.AppId)
        |> Result.bind (tfVar "app_name" message.AppName)
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
            saveStateFile config message.AppId (Terraform.stateFile wd)
            Error(err)
        | Ok(wd) ->
            logger.Info("terraform-applied")
            saveStateFile config message.AppId (Terraform.stateFile wd)
            Ok(wd)
            
let private deployApplication (logger : Api.Config.Logging.Logger) (config : Api.Config.Configuration) (message : Api.Domain.Messages.DeployAppMessage) : obj =
    logger.Info("deploy-application", {|AppId = message.AppId; Version = message.Version|})
    let result =
        createContainerImage logger message
        |> Result.bind (publishContainerImage logger)
        |> Result.bind (terraformDeploy logger config message)
    
    match result with
    | Ok _ -> logger.Info("deployed-application", {|AppId = message.AppId; Version = message.Version|})
    | Error err -> logger.Error("deploy-application", err)
    null

let deployApplicationHandler logger config = asMessageHandler (deployApplication logger config)
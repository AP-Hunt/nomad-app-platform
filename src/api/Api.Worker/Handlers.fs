module Handlers

open System

open ServiceStack.Messaging

open Api.Domain.Buildpacks
open Api.Domain.ContainerImages
open BuildpackExecutor


let asMessageHandler<'T when 'T : not struct> (fn:'T -> Object) : Func<IMessage<'T>, Object> =
    Func<IMessage<'T>, Object>(fun (incoming : IMessage<'T>) -> fn (incoming.GetBody()))
        

let private deployApplication (logger : Api.Config.Logging.Logger) (message : Api.Domain.Messages.DeployAppMessage) : Object =
    
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
    | Ok(imageName) ->
        logger.Info("publish-container-image", {| Image = imageName |})
        match ContainerImages.push(imageName) with
        | Ok(imageReference) -> logger.Info("published-container-image", {| Image = imageReference |})
        | Error(err) ->
            logger.Error("publish-container-image", err)
        
    null

let deployApplicationHandler logger = asMessageHandler (deployApplication logger)
    
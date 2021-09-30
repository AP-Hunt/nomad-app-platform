module Handlers

open System

open ServiceStack.Messaging

open Api.Domain.Buildpacks
open Api.Domain.ContainerImages
open BuildpackExecutor


let asMessageHandler<'T when 'T : not struct> (fn:'T -> Object) : Func<IMessage<'T>, Object> =
    Func<IMessage<'T>, Object>(fun (incoming : IMessage<'T>) -> fn (incoming.GetBody()))
        

let private deployApplication (message : Api.Domain.Messages.DeployAppMessage) : Object =
    Console.WriteLine("Deploying app {0}", message.AppId)
    
    let appImage =
        BuildpackExecutor.defaults
        |> buildpack "gcr.io/paketo-buildpacks/go"
        |> registryAddress "localhost:6000"
        |> sourcePath message.SourcePath
        |> run message.AppId (message.Version.ToString())
    
    match appImage with
    | Error(err) ->
        Console.Error.WriteLine("Error creating application image")
        Console.Error.WriteLine(err)    
    | Ok(imageName) ->
        match ContainerImages.push(imageName) with
        | Ok(imageReference) -> Console.WriteLine($"Created image '{imageReference}' for app guid {message.AppId}")
        | Error(err) ->
            Console.Error.WriteLine("Error pushing application image")
            Console.Error.WriteLine(err)    
        
    null

let deployApplicationHandler = asMessageHandler deployApplication
    
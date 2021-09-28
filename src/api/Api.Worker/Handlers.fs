module Handlers

open System
open ServiceStack.Messaging

let asMessageHandler<'T when 'T : not struct> (fn:'T -> Object) : Func<IMessage<'T>, Object> =
    Func<IMessage<'T>, Object>(fun (incoming : IMessage<'T>) -> fn (incoming.GetBody()))
        

let private deployApplication (message : Api.Domain.Messages.DeployAppMessage) : Object =
    Console.WriteLine("Deploying app {0}", message.AppId)
    null

let deployApplicationHandler = asMessageHandler deployApplication
    
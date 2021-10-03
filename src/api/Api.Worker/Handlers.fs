module Handlers

open System

open ServiceStack.Messaging

let asMessageHandler<'T when 'T : not struct> (fn:'T -> obj) : Func<IMessage<'T>, obj> =
    Func<IMessage<'T>, obj>(fun (incoming : IMessage<'T>) -> fn (incoming.GetBody()))
        
    
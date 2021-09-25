module ResponseHelpers
    
open System.Text.Json
open Suave
open Suave.Json
open Suave.Operators
open Suave.Successful
open Suave.Writers

let JSON object webpart =
    let jsonSerializerOpts = new JsonSerializerOptions()
    jsonSerializerOpts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    jsonSerializerOpts.WriteIndented <- true
    
    let encodeJson obj = JsonSerializer.Serialize(obj, jsonSerializerOpts)
    
    webpart
    >=> OK (object |> encodeJson)
    >=> setMimeType "json"

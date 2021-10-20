namespace Api.Domain.Applications

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type Application =
    {
        Id: Guid
        Name: string
        Version: int
    }

module Applications =     
    let incrementVersion application =
        { application with Version = application.Version + 1}
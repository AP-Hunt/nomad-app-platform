namespace Api.Domain.Applications

open System

type Application =
    {
        Id: string option
        Name: string
        Version: int
    }

module Applications =
    let generateId application =
        { application with Id = Some(Guid.NewGuid().ToString()) }
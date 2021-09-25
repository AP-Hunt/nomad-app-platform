module AppRoutes
    open Suave
    open Suave.Filters
    open Suave.Json
    open Suave.Operators
    open Suave.Successful
    
    open Api.Web.Pagination
    open Api.Domain.Applications
    open PathHelpers
    open Pagination
    open ResponseHelpers

    let listOfApps =
        [
            { Application.Id = "123"; Name = "app-1"; }
            { Application.Id = "456"; Name = "app-2"; }
        ]
    
    let apps_index =
        trailingSlashPath "/apps" >=> choose [
            GET |> JSON (listOfApps |> paginatedCollectionOf)
        ]

    let routes =
        choose
            [
                apps_index
            ]
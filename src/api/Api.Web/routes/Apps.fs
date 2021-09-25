module AppRoutes
    open System.IO
    
    open Api.Domain.SourceArchive
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
    
    let upload_app_handler request =
        let tempFileStream = File.Open(request.files.Head.tempFilePath, FileMode.Open)
        
        match SourceArchive.validateArchive tempFileStream with
        | Ok _ -> Successful.OK("valid")
        | Error e -> RequestErrors.BAD_REQUEST("Invalid archive")
    
    let apps_index =
        trailingSlashPath "/apps" >=> choose [
            GET |> JSON (listOfApps |> paginatedCollectionOf)
            POST >=> request upload_app_handler
        ]

    let routes =
        choose
            [
                apps_index
            ]
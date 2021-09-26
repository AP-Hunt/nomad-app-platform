module AppRoutes
    open Microsoft.AspNetCore.Http
    
    open Falco
    open Falco.Routing
    
    open Api.Domain.SourceArchive
    open Api.Web.Pagination
    open Api.Domain.Applications
    open Pagination
    

    let listOfApps =
        [
            { Application.Id = "123"; Name = "app-1"; }
            { Application.Id = "456"; Name = "app-2"; }
        ]
    
    let apps_index_post : HttpHandler =
        let formBinder (f : FormCollectionReader) : IFormFile option =
            f.TryGetFormFile "source_bundle"
            
        let uploadToTempStore (sourceBundle : IFormFile option) : HttpHandler =
            match sourceBundle with
            | Some (bundle) ->
                match SourceArchive.validateArchive (bundle.OpenReadStream()) with
                | Ok _ ->
                    Response.withStatusCode StatusCodes.Status202Accepted
                    >> Response.ofJson {|Success = true; Error = "" |}
                | Error e ->
                    Response.withStatusCode StatusCodes.Status400BadRequest
                    >> Response.ofJson {|Success = false; Error = e.ToString() |}
            | None ->
                Response.withStatusCode(StatusCodes.Status400BadRequest)
                >> Response.ofJson {|Success = false; Error = "No archive uploaded"|} 
            
        Request.mapFormStream formBinder uploadToTempStore
    
    let apps_index_get : HttpHandler = Response.ofJson (listOfApps |> paginatedCollectionOf)
    
    let all = [
        get "/apps" apps_index_get
        post "/apps" apps_index_post
    ]
    
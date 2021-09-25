namespace Api.Web.Pagination
open System.Runtime.Serialization

[<DataContract>]
type PaginationDetails =
    {
        [<field: DataMember>]
        CurrentPage: int
        [<field: DataMember>]
        TotalPages: int
        [<field: DataMember>]
        PerPage: int
        [<field: DataMember>]
        TotalItems: int
    }

[<DataContract>]
type PaginatedCollection<'T> =
    {
        [<field: DataMember>]
        Pagination: PaginationDetails
        [<field: DataMember>]
        Items: 'T[]
    }

module Pagination =
    let paginatedCollectionOf xs =
        {
            Pagination = {
                CurrentPage = 1;
                TotalPages = 1;
                PerPage = (xs |> List.length);
                TotalItems = (xs |> List.length);
            }
            Items = xs |> Array.ofList
        }
        
    let pageNumber pageNum collection = { collection.Pagination with CurrentPage = pageNum }
    let totalPages totalPages collection = { collection.Pagination with TotalPages = totalPages }
    let perPage perPage collection = { collection.Pagination with PerPage = perPage }
    let totalItems totalItems collection = { collection.Pagination with TotalItems = totalItems }
    
namespace Api.Web.Services

open System

type EnvironmentService() =
    member val SourceBundleBlobStore = Environment.GetEnvironmentVariable("SOURCE_BUNDLE_BLOB_STORE") with get
    
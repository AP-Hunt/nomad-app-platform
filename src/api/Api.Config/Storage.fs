namespace Api.Config

open System.IO


module Storage =
    let private ensurePath path =
        match Directory.Exists(path) with
        | true -> ()
        | false ->
            Directory.CreateDirectory(path) |> ignore

    let ensureStoragePaths config =
        [
            config.BlobStore.SourceBundleStoragePath;
            config.BlobStore.TerraformStatePath;
            config.Logging.LogPath;
        ]
        |> List.iter ensurePath
namespace Api.Domain.SourceArchive

open System
open System.IO
open System.Linq
open SharpCompress.Archives
open SharpCompress.Archives

type ValidationError =
    | InvalidArchive
    | EmptyArchive
    | MissingManifest

// Module SourceArchive handles validation and actions
// against archives of user-submitted source code
module public SourceArchive =
    
    let private tryFindManifestFile (archive : IArchive) = 
        archive.Entries
        |> List.ofSeq
        |> List.tryFind (fun e -> e.Key = "manifest.yml")
        
    let validateArchive (stream : IO.Stream) =
        try
            let isNotEmpty (archive : IArchive) =
                match archive.Entries.Count() with
                | 0 -> Error EmptyArchive
                | _ -> Ok archive

            let containsManifestFile (archive: IArchive) =
                match tryFindManifestFile archive with
                | Some _ -> Ok(archive)
                | None -> Error MissingManifest
   
            let validate (archive: IArchive) =
                isNotEmpty archive
                |> Result.bind containsManifestFile
                
            using (ArchiveFactory.Open(stream)) validate
        with
        | _ -> Error InvalidArchive
        
    let extractManifest (archive : IArchive) = 
        match tryFindManifestFile archive with
        | Some entry ->
            let text = (new StreamReader(entry.OpenEntryStream())).ReadToEnd()
            Some(text)
        | None -> None
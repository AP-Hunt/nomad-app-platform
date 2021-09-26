module Api.Domain.SourceArchiveTest

open System.IO
open System.Text

open NUnit.Framework

open Api.Domain.SourceArchive

let toStream (string : string) =
    let strm = new MemoryStream()
    let stringBytes = Encoding.UTF8.GetBytes(string)
    strm.Write(stringBytes, 0, stringBytes.Length)
    strm.Position <- 0L
    strm

let emptyZipArchive () =
    let memStream = new MemoryStream()
    let zip = SharpCompress.Archives.Zip.ZipArchive.Create()
    using zip ( fun archive ->
        archive.SaveTo(memStream)
    )
    
    memStream.Position <- (int64)0
    memStream
    
let archiveWithoutManifestFile () =
    let memStream = new MemoryStream()
    let zip = SharpCompress.Archives.Zip.ZipArchive.Create()
    using zip ( fun archive ->
        archive.AddEntry("not-the-manifest.yml", "some yaml" |> toStream) |> ignore
        archive.SaveTo(memStream)
    )
    
    memStream.Position <- (int64)0
    memStream
    
let exampleManifest = """
---
name: app-1
"""

let archiveFileValidManifestFile () =
    let memStream = new MemoryStream()
    let zip = SharpCompress.Archives.Zip.ZipArchive.Create()
    zip.AddEntry("manifest.yml", exampleManifest |> toStream) |> ignore
    zip
    
let expectError expected actual =
    match actual with
    | Ok _ -> Assert.Fail ()
    | Error e ->
        Assert.AreEqual(expected, e)
    
let pass _ = Assert.Pass()    
let fail _ = Assert.Fail()    
    
[<TestFixture>]
type ValidateArchiveTest () = 
    [<Test>]
    member this.``Returns an error when the file is not a valid zip archive`` () =
        let stream = new MemoryStream()
        
        SourceArchive.validateArchive stream
        |> expectError ValidationError.InvalidArchive
            
    [<Test>]
    member this.``Returns an error when the archive is empty`` () =
        SourceArchive.validateArchive (emptyZipArchive ())
        |> expectError ValidationError.EmptyArchive
        
    [<Test>]
    member this.``Returns an error when the manifest file is missing`` () =
        SourceArchive.validateArchive (archiveWithoutManifestFile ())
        |> expectError ValidationError.MissingManifest
    
[<TestFixture>]        
type ExtractManifestTest () =
    
    [<Test>]
    member this.``Extracts the content of the manifest file as a string`` () =
        let archive = archiveFileValidManifestFile ()
        let content = SourceArchive.extractManifest archive
        Assert.AreEqual(Some(exampleManifest), content)
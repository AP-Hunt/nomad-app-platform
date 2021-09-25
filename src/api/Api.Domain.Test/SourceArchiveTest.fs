module Api.Domain.SourceArchiveTest

open System.IO

open System.Text
open Api.Domain.SourceArchive
open NUnit.Framework
open SharpCompress.Writers

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
    
let expectError expected actual =
    match actual with
    | Ok _ -> Assert.Fail ()
    | Error e ->
        Assert.AreEqual(expected, e)
    
let pass _ = Assert.Pass()    
let fail _ = Assert.Fail()    
    
[<SetUp>]
let Setup () =
    ()

[<Test>]
let ``Returns an error when the file is not a valid zip archive`` () =
    let stream = new MemoryStream()
    
    SourceArchive.validateArchive stream
    |> expectError ValidationError.InvalidArchive
        
[<Test>]
let ``Returns an error when the archive is empty`` () =
    SourceArchive.validateArchive (emptyZipArchive ())
    |> expectError ValidationError.EmptyArchive
    
[<Test>]
let ``Returns an error when the manifest file is missing`` () =
    SourceArchive.validateArchive (archiveWithoutManifestFile ())
    |> expectError ValidationError.MissingManifest
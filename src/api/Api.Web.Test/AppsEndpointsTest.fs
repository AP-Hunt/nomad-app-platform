namespace Api.Web.Test

module AppsEndpointsTest =

    open System.Diagnostics
    open System.IO
    open System.Net.Http
    open System.Net
    open System.Text
    open System.Threading

    open NUnit.Framework

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

    let zipWithoutManifest () =
        let memStream = new MemoryStream()
        let zip = SharpCompress.Archives.Zip.ZipArchive.Create()
        using zip ( fun archive ->
            archive.AddEntry("not-the-manifest.yml", "some yaml" |> toStream) |> ignore
            archive.SaveTo(memStream)
        )
        
        memStream.Position <- (int64)0
        memStream
        
    let zipWithManifest () =
        let memStream = new MemoryStream()
        let zip = SharpCompress.Archives.Zip.ZipArchive.Create()
        using zip ( fun archive ->
            archive.AddEntry("manifest.yml", "some yaml" |> toStream) |> ignore
            archive.SaveTo(memStream)
        )
        
        memStream.Position <- (int64)0
        memStream        
        
    let mutable server : Process = null

    [<TestFixture>]
    type AppsIndexTests() =
        [<SetUp>]
        member this.setup () =  
            let startInfo =
                ProcessStartInfo(
                    UseShellExecute = false,
                    FileName = "Api.Web"
                )
            server <- new Process(StartInfo = startInfo)
            server.Start() |> ignore
            Thread.Sleep(1000) // Give the server 1 second to start up
            
        [<TearDown>]
        member this. teardown () =
            server.Kill()
            
        [<Test>]
        member this.``POST /apps with an empty zip returns HTTP 400`` () =
            let zipStream = emptyZipArchive ()
            
            use client = new HttpClient()   
            use formData = new MultipartFormDataContent()
            use content = new StreamContent(zipStream)
            
            formData.Add(content, "source_bundle", "source_bundle.zip")
                 
            let url = "http://localhost:5000/apps/"
            let response = client.PostAsync(url, formData) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode)
            
        [<Test>]
        member this.``POST /apps with a zip not containing a manifest returns HTTP 400`` () =
            let zipStream = zipWithoutManifest ()
            
            use client = new HttpClient()   
            use formData = new MultipartFormDataContent()
            use content = new StreamContent(zipStream)
            
            formData.Add(content, "source_bundle", "source_bundle.zip")
                 
            let url = "http://localhost:5000/apps/"
            let response = client.PostAsync(url, formData) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode)
            
        [<Test>]
        member this.``POST /apps with a valid zip returns HTTP 201`` () =
            let zipStream = zipWithManifest ()
            
            use client = new HttpClient()   
            use formData = new MultipartFormDataContent()
            use content = new StreamContent(zipStream)
            
            formData.Add(content, "source_bundle", "source_bundle.zip")
                 
            let url = "http://localhost:5000/apps/"
            let response = client.PostAsync(url, formData) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode)                        
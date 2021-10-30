namespace Api.Domain.Buildpacks

open System.Diagnostics
    
type Settings =
    {
        Builder : string option
        Buildpack : string option
        SourcePath : string option
        RegistryAddress : string option
    }
    
module BuildpackExecutor =

    open Result
        
    let defaults =
        {
            Builder = Some("paketobuildpacks/builder:base")
            Buildpack = None
            SourcePath = None
            RegistryAddress = None
        }
    
    let builder builder settings =
        {settings with Settings.Builder = Some(builder)}
        
    let buildpack buildpack settings =
        {settings with Settings.Buildpack = Some(buildpack)}
        
    let sourcePath sourcePath settings =
        {settings with Settings.SourcePath = Some(sourcePath)}
        
    let registryAddress registryAddress settings =
        {settings with Settings.RegistryAddress = Some(registryAddress)}       
        
    let private imageTag name version registryAddress =
       $"%s{registryAddress}/%s{name}:%s{version}"
        
    let run name version settings : Result<string, string>=
        let imageName = (imageTag name version (settings.RegistryAddress |> Option.get))
        let startInfo =
            ProcessStartInfo(
                UseShellExecute = false,
                FileName = "pack",
                RedirectStandardOutput = true,
                Arguments = (String.concat " " [
                    "build";
                    imageName;
                    "--quiet";
                    "--builder"; settings.Builder |> Option.get ;
                    "--buildpack"; settings.Buildpack |> Option.get ;
                    "--path"; settings.SourcePath |> Option.get
                    "--publish";
                ])
            )
            
        let packProcess = new Process(StartInfo = startInfo)
        packProcess.Start() |> ignore
        packProcess.WaitForExit()
        
        match packProcess.ExitCode with
        | 0 -> Ok(imageName)
        | _ -> Error(packProcess.StandardError.ReadToEnd().Trim())
        
        
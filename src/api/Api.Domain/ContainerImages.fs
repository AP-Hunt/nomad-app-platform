namespace Api.Domain.ContainerImages

open System.Diagnostics

module ContainerImages =
    let push imageName =
        let startInfo =
            ProcessStartInfo(
                UseShellExecute = false,
                FileName = "docker",
                RedirectStandardOutput = true,
                Arguments = (String.concat " " [
                    "push"
                    "--quiet";
                    imageName
                ])
            )
            
        let dockerProcess = new Process(StartInfo = startInfo)
        dockerProcess.Start() |> ignore
        dockerProcess.WaitForExit()
        
        match dockerProcess.ExitCode with
        | 0 -> Ok(dockerProcess.StandardOutput.ReadToEnd().Trim())
        | _ -> Error(dockerProcess.StandardError.ReadToEnd().Trim())

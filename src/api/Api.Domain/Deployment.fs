namespace Api.Domain.Deployment

open System.Diagnostics
open System.IO

module Terraform =
    
    let private tf (args : string list) workingDir =
        let startInfo =
            ProcessStartInfo(
                UseShellExecute = false,
                FileName = "terraform",
                RedirectStandardOutput = true,
                Arguments = (String.concat " " args),
                WorkingDirectory = workingDir
            )
            
        let tfProcess = new Process(StartInfo = startInfo)
        tfProcess.Start() |> ignore
        tfProcess.WaitForExit()
        
        match tfProcess.ExitCode with
        | 0 -> Ok(workingDir)
        | _ -> Error(tfProcess.StandardOutput.ReadToEnd().Trim())
        
    let prepareWorkingDirectory path : Result<string, string>=
        let workingDir = Directory.CreateDirectory(Path.Combine(path, Path.GetRandomFileName()))
        let terraformDir = Path.Combine(Directory.GetCurrentDirectory(), "terraform")
        
        Directory.GetFiles(terraformDir)
        |> Array.iter(fun f ->
            let destFile = Path.Combine(workingDir.FullName, Path.GetFileName(f))
            File.Copy(f, destFile, true)
        )
        
        Ok(workingDir.FullName)
        
    let var (name : string) (value : string) workingDir =
        let varsFile = Path.Combine(workingDir, "vars.tfvars")
        match File.Exists(varsFile) with
        | true -> ()
        | false ->
            let f = File.Create(varsFile)
            f.Close()
        
        File.AppendAllLines(varsFile, [$"{name}=\"{value}\""])
        Ok(workingDir)
        
    let stateFile workingDir =
        Path.Combine(workingDir, "terraform.tfstate")
        
    let apply workingDir =
        let applicationResult =
            tf ["init"] workingDir
            |> Result.bind (
                tf ["apply"; "-var-file"; "vars.tfvars"; "-input=false"; "-auto-approve"]
            )
            
        match applicationResult with
        | Ok wd -> Ok wd
        | Error err -> Error((workingDir, err))
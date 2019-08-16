open System
open System.IO
open NuGet.ProjectModel
open NuGet.Common
open NuGet.Protocol
open System.Diagnostics
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NuGet.Packaging.Core

let generateDependencyGraph projectPath =
    async {
        let tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName())

        let args =
            [| "msbuild"
               projectPath
               "/t:GenerateRestoreGraphFile"
               sprintf "/p:RestoreGraphOutputPath=%s" tempPath |]

        let psi = ProcessStartInfo("/usr/bin/dotnet", args |> String.concat " ")
        psi.WorkingDirectory <- Environment.CurrentDirectory
        psi.UseShellExecute <- false
        psi.CreateNoWindow <- true
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        let p = new Process()
        p.StartInfo <- psi
        p.Start() |> ignore
        let rec consumeStream (reader : StreamReader) (output : StringBuilder) =
            async {
                let! line = reader.ReadLineAsync() |> Async.AwaitTask
                match line with
                | null -> return output
                | l -> return! output.AppendLine l |> consumeStream reader
            }

        let oa = consumeStream p.StandardOutput (StringBuilder())
        let ea = consumeStream p.StandardError (StringBuilder())
        match p.WaitForExit 20000 with
        | false ->
            p.Kill()
            return None
        | true ->
            let! _ = [| oa; ea |] |> Async.Parallel
            return Some(File.ReadAllText tempPath)
    }

[<EntryPoint>]
let main _ =
    let dg =
        Path.Combine(Environment.CurrentDirectory, "DotNetDelice.fsproj")
        |> generateDependencyGraph
        |> Async.RunSynchronously
    match dg with
    | None -> printfn "whoops"
    | Some s ->
        let dependencyGraph = JsonConvert.DeserializeObject<JObject> s |> DependencyGraphSpec.Load
        let projectSpec = dependencyGraph.Projects |> Seq.find (fun p -> p.Name = "DotNetDelice")
        let file = Path.Combine(projectSpec.RestoreMetadata.OutputPath, "project.assets.json")
        let lockFile = LockFileUtilities.GetLockFile(file, NullLogger.Instance)
        lockFile.Libraries
        |> Seq.iter (fun lib ->
               let identity = PackageIdentity(lib.Name, lib.Version)
               let pi =
                   LocalFolderUtility.GetPackageV3
                       (projectSpec.RestoreMetadata.PackagesPath, identity, NullLogger.Instance)
               match pi with
               | null -> ignore()
               | _ ->
                   printfn "Looking for licence for %A" identity
                   match pi.Nuspec.GetLicenseMetadata() with
                   | null ->
                       printfn "No licence metadata found"
                       printfn "Legacy licence style via URL: %s" <| pi.Nuspec.GetLicenseUrl()
                   | licence ->
                       printfn "Licence metadata found"
                       printfn "Licence: %A, Version: %A" licence.License licence.Version
                   printfn "")
    0 // return an integer exit code

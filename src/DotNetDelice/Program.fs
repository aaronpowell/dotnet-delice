open System
open System.IO
open NuGet.ProjectModel
open NuGet.Common
open NuGet.Protocol
open NuGet.Packaging.Core
open DependencyGraph

[<EntryPoint>]
let main _ =
    let dg =
        Path.Combine(Environment.CurrentDirectory, "DotNetDelice.fsproj")
        |> generateDependencyGraph
        |> Async.RunSynchronously
    match dg with
    | None -> printfn "whoops"
    | Some dependencyGraph ->
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

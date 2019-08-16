open System
open System.IO
open NuGet.ProjectModel
open NuGet.Common
open DependencyGraph
open LicenseBuilder

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
        let licenses = lockFile.Libraries |> Seq.map (getPackageLicense projectSpec)

        let unlicensed =
            licenses
            |> Seq.choose (fun l ->
                   match l with
                   | PackageNotFound l -> Some l
                   | _ -> None)

        let licensed =
            licenses
            |> Seq.choose (fun l ->
                   match l with
                   | Licensed l -> Some l
                   | _ -> None)

        let legacyLicensed =
            licenses
            |> Seq.choose (fun l ->
                   match l with
                   | LegacyLicensed l -> Some l
                   | _ -> None)

        printfn "Unlicensed: %A" unlicensed
        printfn "Licensed: %A" licensed
        printfn "Legacy Licensed: %A" legacyLicensed
        licensed
        |> Seq.groupBy (fun license -> license.Type)
        |> Seq.choose (fun (g, l) ->
               match g with
               | Some g -> Some(g, l)
               | None -> None)
        |> Seq.iter (fun (g, licenses) ->
               printfn "License: %s" g
               printfn "%A" licenses)
    0 // return an integer exit code

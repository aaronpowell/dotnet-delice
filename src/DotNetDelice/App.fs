[<AutoOpen>]
module App

open McMaster.Extensions.CommandLineUtils
open System
open System.IO
open NuGet.ProjectModel
open NuGet.Common
open DependencyGraph
open LicenseBuilder

let (|Proj|_|) arg =
    let c str = String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
    if c ".sln" then Some()
    elif c ".csproj" then Some()
    elif c ".fsproj" then Some()
    else None

let findProject path =
    match (File.Exists path, Directory.Exists path) with
    | (false, false) -> failwith "The path provided does not exist"
    | (true, _) ->
        match Path.GetExtension path with
        | Proj -> Path.GetFullPath path
        | _ -> failwith "Path is not a valid project or solution path"
    | (_, true) ->
        match Directory.GetFiles(path, "*.sln") with
        | [| sln |] -> Path.GetFullPath sln
        | [||] ->
            match Directory.GetFiles(path, "*.csproj")
                  |> Array.append
                  <| Directory.GetFiles(path, "*.fsproj") with
            | [| proj |] -> Path.GetFullPath proj
            | [||] -> failwith "No project files found in the path"
            | _ -> failwith "More than one project files found in the path"
        | _ -> failwith "More than one solution file found in the path"

[<HelpOption>]
type Cli() =

    [<Argument(0,
               Description = "The path to a .sln, .csproj or .fsproj file, or to a directory containing a .NET Core solution/project. If none is specified, the current directory will be used.")>]
    member val Path = "" with get, set

    member this.OnExecute() =
        let path =
            match this.Path with
            | ""
            | null -> Environment.CurrentDirectory
            | _ -> this.Path

        let dg =
            findProject path
            |> generateDependencyGraph
            |> Async.RunSynchronously

        match dg with
        | None -> printfn "whoops"
        | Some dependencyGraph ->
            dependencyGraph.Projects
            |> Seq.iter (fun projectSpec ->
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

                   printfn "Project %s" projectSpec.Name
                   printfn "Unlicensed: %s" (unlicensed
                                             |> Seq.map (fun l -> l.PackageName)
                                             |> String.concat "; ")
                   printfn "Legacy Licensed: %s" (legacyLicensed
                                                  |> Seq.map (fun l -> l.PackageName)
                                                  |> String.concat "; ")
                   licensed
                   |> Seq.groupBy (fun license -> license.Type)
                   |> Seq.choose (fun (g, l) ->
                          match g with
                          | Some g -> Some(g, l)
                          | None -> None)
                   |> Seq.iter (fun (g, licenses) ->
                          printfn "License name: %s" g
                          printfn "%s" (licenses
                                        |> Seq.map (fun l -> l.PackageName)
                                        |> String.concat "; "))
                   printfn "")

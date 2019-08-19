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

    [<OptionAttribute(Description = "Output result as JSON")>]
    member val Json = false with get, set

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
            if this.Json then
                dependencyGraph.Projects
                |> Seq.map (fun projectSpec ->
                       let file = Path.Combine(projectSpec.RestoreMetadata.OutputPath, "project.assets.json")
                       let lockFile = LockFileUtilities.GetLockFile(file, NullLogger.Instance)
                       let licenses = lockFile.Libraries |> Seq.map (getPackageLicense projectSpec)
                       jsonBuilder projectSpec licenses)
                |> jsonPrinter
            else
                dependencyGraph.Projects
                |> Seq.iter (fun projectSpec ->
                       let file = Path.Combine(projectSpec.RestoreMetadata.OutputPath, "project.assets.json")
                       let lockFile = LockFileUtilities.GetLockFile(file, NullLogger.Instance)
                       let licenses = lockFile.Libraries |> Seq.map (getPackageLicense projectSpec)
                       prettyPrint projectSpec licenses
                       printfn "")

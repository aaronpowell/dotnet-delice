[<AutoOpen>]
module App

open McMaster.Extensions.CommandLineUtils
open System
open System.ComponentModel.DataAnnotations
open System.IO
open NuGet.ProjectModel
open NuGet.Common
open DependencyGraph
open LicenseBuilder
open Spdx

let (|Proj|_|) arg =
    let c str =
        String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0

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
            match
                Directory.GetFiles(path, "*.csproj")
                |> Array.append
                <| Directory.GetFiles(path, "*.fsproj")
                with
            | [| proj |] -> Path.GetFullPath proj
            | [||] -> failwith "No project files found in the path"
            | _ -> failwith "More than one project files found in the path"
        | _ -> failwith "More than one solution file found in the path"

let getLicenses checkGitHub token checkLicenseContent similarity (projectSpec: PackageSpec) =
    let file =
        Path.Combine(projectSpec.RestoreMetadata.OutputPath, "project.assets.json")

    let lockFile = LockFileUtilities.GetLockFile(file, NullLogger.Instance)

    lockFile.Libraries
    |> Seq.map (fun lib ->
        getPackageLicense projectSpec checkGitHub token checkLicenseContent lib.Name lib.Version lib.Type similarity)

let getLicensesForTool checkGitHub token checkLicenseContent (projectSpec: PackageSpec) similarity =
    let package =
        projectSpec.TargetFrameworks
        |> Seq.map (fun fx ->
            let dep = fx.Dependencies |> Seq.head

            let pkg =
                NuGet.Protocol.LocalFolderUtility.GetPackageV3(
                    projectSpec.RestoreMetadata.PackagesPath,
                    dep.Name,
                    dep.LibraryRange.VersionRange.MinVersion,
                    MemoryLogger.Instance
                )

            pkg)
        |> Seq.head

    let depGroups = package.Nuspec.GetDependencyGroups()

    depGroups
    |> Seq.collect (fun dg -> dg.Packages)
    |> Seq.map (fun p ->
        getPackageLicense
            projectSpec
            checkGitHub
            token
            checkLicenseContent
            p.Id
            p.VersionRange.MinVersion
            "package"
            similarity)

[<HelpOption>]
type Cli() =

    [<Argument(0,
               Description = "The path to a .sln, .csproj or .fsproj file, or to a directory containing a .NET Core solution/project. If none is specified, the current directory will be used.")>]
    member val Path = "" with get, set

    [<Option(Description = "Output result as JSON")>]
    member val Json = false with get, set

    [<Option("--json-output", Description = "Path to JSON file rather than stdout")>]
    member val JsonOutput = "" with get, set

    [<Option("--check-github",
             Description = "If provided delice will attempt to look up the license via the GitHub API (provided it's GitHub hosted)")>]
    member val CheckGitHub = false with get, set

    [<Option("--github-token",
             Description = "A GitHub Personal Access Token to perform authenticated requests against the API (used with --check-github). This ensures the tool isn't rate-limited when running")>]
    member val GitHubToken = "" with get, set

    [<Option("--check-license-content",
             Description = "When set delice will attempt to look at the license text and match it against some known license templates")>]
    member val CheckLicenseContent = false with get, set

    [<Option("--refresh-spdx", Description = "Refreshes the SPDX license.json file used by the tool")>]
    member val RefreshSpdx = false with get, set

    [<Option("--similarity",
             Description = "The level of similarity to apply when comparing license contents to the SPDX templates")>]
    [<Range(0, 1)>]
    member val Similarity = 0.9 with get, set

    member this.OnExecute() =
        if this.RefreshSpdx then
            getSpdx true |> Async.RunSynchronously |> ignore

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
        | None ->
            printfn "Failed to generate the dependency graph for '%s'." path
            printfn "Ensure that the project has been restored and compiled before running delice."

            printfn
                "delice only supports SDK project files (.NET Core, NETStandard, etc.), not legacy MSBuild ones (common for .NET Framework)."
        | Some dependencyGraph ->
            let getLicenses' =
                getLicenses this.CheckGitHub this.GitHubToken this.CheckLicenseContent this.Similarity

            let projects =
                dependencyGraph.Projects
                |> Seq.filter (fun projectSpec ->
                    projectSpec.RestoreMetadata.ProjectStyle
                    <> ProjectStyle.Unknown
                    && projectSpec.RestoreMetadata.ProjectStyle
                       <> ProjectStyle.DotnetCliTool)

            if this.Json then
                let toolLicenseResults =
                    dependencyGraph.Projects
                    |> Seq.filter (fun ps -> ps.RestoreMetadata.ProjectStyle = ProjectStyle.DotnetCliTool)
                    |> Seq.map (fun projectSpec ->
                        getLicensesForTool
                            this.CheckGitHub
                            this.GitHubToken
                            this.CheckLicenseContent
                            projectSpec
                            this.Similarity
                        |> jsonBuilder projectSpec.Name)

                projects
                |> Seq.map (fun projectSpec ->
                    getLicenses' projectSpec
                    |> jsonBuilder projectSpec.Name)
                |> Seq.append toolLicenseResults
                |> jsonPrinter this.JsonOutput
            else
                projects
                |> Seq.iter (fun projectSpec ->
                    getLicenses' projectSpec
                    |> prettyPrint projectSpec.Name

                    printfn "")

                dependencyGraph.Projects
                |> Seq.filter (fun ps -> ps.RestoreMetadata.ProjectStyle = ProjectStyle.DotnetCliTool)
                |> Seq.iter (fun projectSpec ->
                    getLicensesForTool
                        this.CheckGitHub
                        this.GitHubToken
                        this.CheckLicenseContent
                        projectSpec
                        this.Similarity
                    |> prettyPrint projectSpec.Name

                    printfn "")

                let unknownProjectStyles =
                    dependencyGraph.Projects
                    |> Seq.filter (fun projectSpec -> projectSpec.RestoreMetadata.ProjectStyle = ProjectStyle.Unknown)

                if Seq.length unknownProjectStyles > 1 then
                    printfn "The following projects were skipped as they are of an unsupported project style:"

                    unknownProjectStyles
                    |> Seq.iteri (fun i projectSpec ->
                        let prefix =
                            if i = Seq.length unknownProjectStyles - 1 then
                                "└"
                            else
                                "├"

                        printfn "%s── %s" prefix projectSpec.Name)

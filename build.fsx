#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.BuildServer

Target.initEnvironment ()

let sln = "./DotNetDelice.sln"

let getChangelog () =
    let changelog = "CHANGELOG.md" |> Changelog.load
    changelog.LatestEntry

let isRelease (targets: Target list) =
    targets
    |> Seq.map (fun t -> t.Name)
    |> Seq.exists ((=) "Release")

let configuration (targets: Target list) =
    let defaultVal =
        if isRelease targets then
            "Release"
        else
            "Debug"

    match Environment.environVarOrDefault "CONFIGURATION" defaultVal with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let getVersionNumber (changeLog: Changelog.ChangelogEntry) (targets: Target list) =
    match GitHubActions.Environment.CI false, isRelease targets with
    | (true, true) -> changeLog.NuGetVersion
    | (true, false) -> sprintf "%s-ci-%s" changeLog.NuGetVersion GitHubActions.Environment.RunId
    | (_, _) -> sprintf "%s-local" changeLog.NuGetVersion

Target.create
    "Clean"
    (fun _ ->
        DotNet.exec id "clean" "" |> ignore
        !! "./.nuget" |> Shell.cleanDirs)

Target.create "Restore" (fun _ -> DotNet.restore id sln)

Target.create
    "Build"
    (fun ctx ->
        let changelog = getChangelog ()

        let args =
            [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
              "--no-restore" ]

        DotNet.build
            (fun c ->
                { c with
                      Configuration = configuration (ctx.Context.AllExecutingTargets)
                      Common = c.Common |> DotNet.Options.withAdditionalArgs args })
            sln)

// Target.create
//     "Publish"
//     (fun ctx ->
//         let changelog = getChangelog ()

//         let args =
//             [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
//               "--no-restore"
//               "--no-build" ]

//         DotNet.publish
//             (fun c ->
//                 { c with
//                       Configuration = configuration (ctx.Context.AllExecutingTargets)
//                       Common = c.Common |> DotNet.Options.withAdditionalArgs args })
//             sln)

Target.create
    "Package"
    (fun ctx ->
        let changelog = getChangelog ()

        let args =
            [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
              sprintf "/p:PackageReleaseNotes=\"%s\"" (sprintf "%O" changelog) ]

        DotNet.pack
            (fun c ->
                { c with
                      Configuration = configuration (ctx.Context.AllExecutingTargets)
                      OutputPath = Some "./.nupkg"
                      Common = c.Common |> DotNet.Options.withAdditionalArgs args })
            sln)

Target.create
    "PackageVersion"
    (fun _ ->
        let version = getChangelog ()
        printfn "The version is %s" version.NuGetVersion)

Target.create
    "Changelog"
    (fun _ ->
        let changelog = getChangelog ()
        Directory.ensure "./.nupkg"

        [| sprintf "%O" changelog |]
        |> File.append "./.nupkg/changelog.md")

Target.create
    "SetVersionForCI"
    (fun _ ->
        let changelog = getChangelog ()
        printfn "::set-env name=package_version::%s" changelog.NuGetVersion)

Target.create "Test" (fun _ -> printfn "Should write some tests...")

Target.create "Default" ignore
Target.create "Release" ignore
Target.create "CI" ignore

"Clean" ==> "Restore" ==> "Build" ==> "Default"

"Default"
// ==> "Publish"
==> "Test"
==> "Package"
==> "Changelog"
==> "Release"

"Default"
// ==> "Publish"
==> "Test"
==> "Package"
==> "Changelog"
==> "CI"

Target.runOrDefault "Default"

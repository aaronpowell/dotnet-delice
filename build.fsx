#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
    !! "src/**/*.*proj"
    |> Seq.iter (DotNet.build id)
)

Target.create "Changelog" (fun _ ->
  let changelog = "CHANGELOG.md" |> Changelog.load
  Directory.ensure "./.nupkg"

  [|sprintf "%O" changelog.LatestEntry|]
  |> File.append "./.nupkg/changelog.md"

  let s = sprintf "%O" changelog.LatestEntry

  printfn "##vso[task.setvariable variable=packageVersion]%s" changelog.LatestEntry.AssemblyVersion

  Templates.replaceInFiles ["@releaseNotes@", s] [ "./src/DotNetDelice/DotNetDelice.fsproj" ]
)

Target.create "All" ignore

"Clean"
  ==> "Build"
  ==> "All"

Target.runOrDefault "All"

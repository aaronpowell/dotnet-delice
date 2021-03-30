module DependencyGraph

open DotNetCli
open System.IO
open NuGet.ProjectModel

let generateDependencyGraph projectPath =
    let tempPath =
        Path.Combine(Path.GetTempPath(), Path.GetTempFileName())

    let args =
        [| "msbuild"
           projectPath
           "/t:GenerateRestoreGraphFile"
           sprintf "/p:RestoreGraphOutputPath=%s" tempPath |]

    async {
        match! dotnetRun args with
        | result when result.IsSuccess -> return DependencyGraphSpec.Load tempPath |> Some
        | _ -> return None
    }

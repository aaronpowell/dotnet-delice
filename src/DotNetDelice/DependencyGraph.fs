module DependencyGraph

open DotNetCli
open Newtonsoft.Json
open System.IO
open Newtonsoft.Json.Linq
open NuGet.ProjectModel

let generateDependencyGraph projectPath =
    let tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName())

    let args =
        [| "msbuild"
           projectPath
           "/t:GenerateRestoreGraphFile"
           sprintf "/p:RestoreGraphOutputPath=%s" tempPath |]
    async {
        match! dotnetRun args with
        | result when result.IsSuccess ->
            let s = File.ReadAllText tempPath
            return JsonConvert.DeserializeObject<JObject> s
                   |> DependencyGraphSpec.Load
                   |> Some
        | _ -> return None
    }

module DotNetCli

open McMaster.Extensions.CommandLineUtils
open System.Diagnostics
open System.IO
open System.Text

let rec private consumeStream (reader: StreamReader) (output: StringBuilder) =
    async {
        let! line = reader.ReadLineAsync() |> Async.AwaitTask
        match line with
        | null -> return output
        | l -> return! output.AppendLine l |> consumeStream reader
    }

type RunResult =
    { Output: string
      Errors: string
      ExitCode: int }
    member this.IsSuccess = this.ExitCode = 0

let dotnetRun args =
    let exe = DotNetExe.FullPathOrDefault()
    async {
        let psi = ProcessStartInfo(exe, args |> String.concat " ")
        psi.WorkingDirectory <- System.Environment.CurrentDirectory
        psi.UseShellExecute <- false
        psi.CreateNoWindow <- true
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        let p = new Process()
        p.StartInfo <- psi
        p.Start() |> ignore
        let output = StringBuilder()
        let errors = StringBuilder()
        let oa = consumeStream p.StandardOutput output
        let ea = consumeStream p.StandardError errors
        match p.WaitForExit 20000 with
        | false ->
            p.Kill()
            return { Output = output.ToString()
                     Errors = errors.ToString()
                     ExitCode = -1 }
        | true ->
            let! _ = [| oa; ea |] |> Async.Parallel
            return { Output = output.ToString()
                     Errors = errors.ToString()
                     ExitCode = p.ExitCode }
    }

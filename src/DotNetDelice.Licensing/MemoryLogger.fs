module MemoryLogger

open NuGet.Common
open System
open System.Text

type MemoryLogger() =
    let log = StringBuilder()
    let debug = StringBuilder()
    let error = StringBuilder()
    let info = StringBuilder()
    let minimal = StringBuilder()
    let verbose = StringBuilder()
    let warning = StringBuilder()
    interface ILogger with

        member this.Log(level : LogLevel, data : string) : unit =
            data
            |> log.AppendLine
            |> ignore

        member this.Log(message : ILogMessage) : unit =
            message.ToString()
            |> log.AppendLine
            |> ignore

        member this.LogAsync(level : LogLevel, data : string) : Threading.Tasks.Task =
            data
            |> log.AppendLine
            |> ignore
            Threading.Tasks.Task.Delay(0)

        member this.LogAsync(message : ILogMessage) : Threading.Tasks.Task =
            message.ToString()
            |> log.AppendLine
            |> ignore
            Threading.Tasks.Task.Delay(0)

        member this.LogDebug(data : string) : unit =
            data
            |> debug.AppendLine
            |> ignore

        member this.LogError(data : string) : unit =
            data
            |> error.AppendLine
            |> ignore

        member this.LogInformation(data : string) : unit =
            data
            |> info.AppendLine
            |> ignore

        member this.LogInformationSummary(data : string) : unit =
            data
            |> info.AppendLine
            |> ignore

        member this.LogMinimal(data : string) : unit =
            data
            |> minimal.AppendLine
            |> ignore

        member this.LogVerbose(data : string) : unit =
            data
            |> verbose.AppendLine
            |> ignore

        member this.LogWarning(data : string) : unit =
            data
            |> warning.AppendLine
            |> ignore

let Instance = MemoryLogger()
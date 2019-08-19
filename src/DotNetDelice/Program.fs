open McMaster.Extensions.CommandLineUtils

[<EntryPoint>]
let main args =
    let app = new CommandLineApplication<Cli>()
    app.ThrowOnUnexpectedArgument <- false
    app.Conventions.UseDefaultConventions() |> ignore

    app.Execute args

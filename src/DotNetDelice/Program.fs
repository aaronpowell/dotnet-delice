open McMaster.Extensions.CommandLineUtils

[<EntryPoint>]
let main args =
    let app = new CommandLineApplication<Cli>()
    app.UnrecognizedArgumentHandling <- UnrecognizedArgumentHandling.Throw
    app.Conventions.UseDefaultConventions() |> ignore

    app.Execute args

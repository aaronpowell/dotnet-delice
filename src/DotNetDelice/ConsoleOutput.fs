[<AutoOpen>]
module ConsoleOutput

open NuGet.ProjectModel
open BlackFox.ColoredPrintf.ColoredPrintf
open Output

let private prettyPrinter printable =
    colorprintfn "License Expression: $green[%s]" printable.Expression
    colorprintfn "├── There are $yellow[%d] occurrences of $green[%s]" printable.Count printable.Expression
    printfn "├─┬ Conformance:"
    colorprintfn "│ ├── Is OSI Approved: $green[%b]" printable.IsOsi
    colorprintfn "│ ├── Is FSF Free/Libre: $green[%b]" printable.IsFsf
    colorprintfn "│ └── Included deprecated IDs: $green[%b]" printable.IsDeprecatedType
    printfn "└─┬ Packages:"
    printable.Packages
    |> Seq.iteri (fun i l ->
        let prefix =
            if i = (printable.Count - 1) then "└"
            else "├"
        printfn "  %s── %s" prefix l.DisplayName)
    printfn ""

let prettyPrint name licenses =
    let unlicensed, projectReferences, licensed, legacyLicensed = getProjectBreakdown licenses

    printfn "Project %s" name
    if Seq.length projectReferences > 0 then
        colorprintfn "$green[Project References]"
        { Expression = "Project References"
          Count = Seq.length projectReferences
          Packages =
              projectReferences
              |> Seq.map (fun l ->
                  { Name = l.PackageName
                    Version = l.PackageVersion.OriginalVersion
                    Url = None
                    DisplayName = sprintf "%s@%s" l.PackageName l.PackageVersion.OriginalVersion })
          IsOsi = false
          IsFsf = false
          IsDeprecatedType = false }
        |> prettyPrinter
    if Seq.length unlicensed > 0 then
        colorprintfn "$red[Packages without licenses]"
        { Expression = "Missing"
          Count = Seq.length unlicensed
          Packages =
              unlicensed
              |> Seq.map (fun l ->
                  { Name = l.PackageName
                    Version = l.PackageVersion.OriginalVersion
                    Url = None
                    DisplayName = sprintf "%s@%s" l.PackageName l.PackageVersion.OriginalVersion })
          IsOsi = false
          IsFsf = false
          IsDeprecatedType = false }
        |> prettyPrinter
    if Seq.length legacyLicensed > 0 then
        colorprintfn "$yellow[Packages using the legacy NuGet license structure]"
        { Expression = "Unable to determine"
          Count = Seq.length legacyLicensed
          Packages =
              legacyLicensed
              |> Seq.map (fun l ->
                  { Name = l.PackageName
                    Version = l.PackageVersion.OriginalVersion
                    Url = Some l.Url
                    DisplayName = sprintf "%s@%s (%s)" l.PackageName l.PackageVersion.OriginalVersion l.Url })
          IsOsi = false
          IsFsf = false
          IsDeprecatedType = false }
        |> prettyPrinter
    if Seq.length licensed > 0 then
        licensed
        |> Seq.map (fun (license, packages) ->
            let exp =
                match license with
                | Some l -> l
                | None -> "No License"

            let (osi, fsf, dep) = getSpdxInfo exp
            { Expression = exp
              Count = Seq.length packages
              Packages =
                  packages
                  |> Seq.map (fun p ->
                      { Name = p.PackageName
                        Version = p.PackageVersion.OriginalVersion
                        Url = Some p.Url
                        DisplayName = sprintf "%s@%s" p.PackageName p.PackageVersion.OriginalVersion })
              IsOsi = osi
              IsFsf = fsf
              IsDeprecatedType = dep })
        |> Seq.iter prettyPrinter
    ignore()

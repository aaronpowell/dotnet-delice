[<AutoOpen>]
module Output

open LicenseBuilder
open BlackFox.ColoredPrintf.ColoredPrintf
open NuGet.ProjectModel

type private PrintableLicense =
    { Expression : string
      Count : int
      Names : string seq }

// hacky little function I use to code gen the cache
let private licensesCodeGen legacyLicensed =
    legacyLicensed
    |> Seq.groupBy (fun l -> l.Url)
    |> Seq.iter
           (fun (url, pkgs) ->
           printfn "(\"%s\", { Expression = \"\"; Packages = Map.ofList[%s]})" url (pkgs
                                                                                    |> Seq.map
                                                                                           (fun p ->
                                                                                           sprintf "(\"%s\", [\"%A\"])"
                                                                                               p.PackageName
                                                                                               p.PackageVersion)
                                                                                    |> String.concat "; "))

let private prettyPrinter printable =
    colorprintfn "License Expression: $green[%s]" printable.Expression
    colorprintfn "├── There are $yellow[%d] occurances of $green[%s]" printable.Count printable.Expression
    printfn "└─┬ Packages:"
    printable.Names
    |> Seq.iteri (fun i l ->
           let prefix =
               if i = (printable.Count - 1) then "└"
               else "├"
           printfn "  %s── %s" prefix l)
    printfn ""

let prettyPrint (projectSpec : PackageSpec) licenses =
    let unlicensed =
        licenses
        |> Seq.choose (fun l ->
               match l with
               | PackageNotFound l -> Some l
               | _ -> None)
        |> Seq.sortBy (fun l -> l.PackageName)

    let licensed =
        licenses
        |> Seq.choose (fun l ->
               match l with
               | Licensed l -> Some l
               | _ -> None)
        |> Seq.sortBy (fun l -> l.PackageName)
        |> Seq.groupBy (fun license -> license.Type)

    let legacyLicensed =
        licenses
        |> Seq.choose (fun l ->
               match l with
               | LegacyLicensed l -> Some l
               | _ -> None)
        |> Seq.sortBy (fun l -> l.PackageName)

    printfn "Project %s" projectSpec.Name
    if Seq.length unlicensed > 0 then
        colorprintfn "$red[Packages without licenses]"
        { Expression = "Missing"
          Count = Seq.length unlicensed
          Names = unlicensed |> Seq.map (fun l -> l.PackageName) }
        |> prettyPrinter
    if Seq.length legacyLicensed > 0 then
        colorprintfn "$yellow[Packages using the legacy NuGet license structure]"
        { Expression = "Unable to determine"
          Count = Seq.length legacyLicensed
          Names = legacyLicensed |> Seq.map (fun l -> sprintf "%s (%s)" l.PackageName l.Url) }
        |> prettyPrinter
    if Seq.length licensed > 0 then
        licensed
        |> Seq.map (fun (license, packages) ->
               { Expression =
                     match license with
                     | Some l -> l
                     | None -> "No License"
                 Count = Seq.length packages
                 Names = packages |> Seq.map (fun p -> p.PackageName) })
        |> Seq.iter prettyPrinter
    ignore()

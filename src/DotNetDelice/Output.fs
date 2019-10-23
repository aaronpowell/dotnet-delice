[<AutoOpen>]
module Output

open LicenseBuilder
open BlackFox.ColoredPrintf.ColoredPrintf
open NuGet.ProjectModel
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open System.IO
open Spdx

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type Package =
    { Name: string;
      Version: string;
      Url: string; }

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type PrintableLicense =
    { Expression : string
      Count : int
      Packages : Package seq
      IsOsi : bool
      IsFsf : bool
      IsDeprecatedType : bool }


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
                                                                                               p.Name
                                                                                               p.Version)
                                                                                    |> String.concat "; "))

let private prettyPrinter printable =
    colorprintfn "License Expression: $green[%s]" printable.Expression
    colorprintfn "├── There are $yellow[%d] occurances of $green[%s]" printable.Count printable.Expression
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
           printfn "  %s── %s (%s)" prefix l.Name l.Url)
    printfn ""

let getSpdxInfo licenseId =
    async {
        let! spdx = getSpdx false
        match spdx.Licenses |> Array.tryFind (fun l -> l.LicenseId = licenseId) with
        | Some spdxInfo -> return (spdxInfo.IsOsiApproved, spdxInfo.IsFsfLibre, spdxInfo.IsDeprecatedLicenseId)
        | None -> return (false, false, false)
    }
    |> Async.RunSynchronously

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
          Packages = unlicensed |> Seq.map (fun l -> { Name = l.PackageName; Version = l.PackageVersion.OriginalVersion; Url = null })
          IsOsi = false
          IsFsf = false
          IsDeprecatedType = false }
        |> prettyPrinter
    if Seq.length legacyLicensed > 0 then
        colorprintfn "$yellow[Packages using the legacy NuGet license structure]"
        { Expression = "Unable to determine"
          Count = Seq.length legacyLicensed
          Packages = legacyLicensed |> Seq.map (fun l -> { Name = l.PackageName; Version = l.PackageVersion.OriginalVersion; Url = l.Url })
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
                 Packages = packages |> Seq.map (fun p -> { Name = p.PackageName; Version = p.PackageVersion.OriginalVersion; Url = p.Url })
                 IsOsi = osi
                 IsFsf = fsf
                 IsDeprecatedType = dep })
        |> Seq.iter prettyPrinter
    ignore()

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type PackageOutput =
    { ProjectName : string
      Licenses : PrintableLicense seq }

let jsonBuilder (projectSpec : PackageSpec) licenses =
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

    let pl =
        Seq.append (if Seq.length unlicensed > 0 then
                        [| { Expression = "Missing"
                             Count = Seq.length unlicensed
                             Packages = unlicensed |> Seq.map (fun l -> { Name = l.PackageName; Version = l.PackageVersion.OriginalVersion; Url = null })
                             IsOsi = false
                             IsFsf = false
                             IsDeprecatedType = false } |]
                    else [||]) (if Seq.length legacyLicensed > 0 then
                                    [| { Expression = "Unable to determine"
                                         Count = Seq.length legacyLicensed
                                         Packages =
                                             legacyLicensed |> Seq.map (fun l -> { Name = l.PackageName; Version = l.PackageVersion.OriginalVersion; Url = l.Url })
                                         IsOsi = false
                                         IsFsf = false
                                         IsDeprecatedType = false } |]
                                else [||])

    { ProjectName = projectSpec.Name
      Licenses =
          licensed
          |> Seq.map (fun (license, packages) ->
                 let exp =
                     match license with
                     | Some l -> l
                     | None -> "No License"

                 let (osi, fsf, dep) = getSpdxInfo exp
                 { Expression = exp
                   Count = Seq.length packages
                   Packages = packages |> Seq.map (fun p -> { Name = p.PackageName; Version = p.PackageVersion.OriginalVersion; Url = p.Url })
                   IsOsi = osi
                   IsFsf = fsf
                   IsDeprecatedType = dep })
          |> Seq.append pl }

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type ProjectOutput =
    { Projects : PackageOutput seq }

let jsonPrinter path json =
    let j = JsonConvert.SerializeObject({ Projects = json }, Formatting.Indented)
    match path with
    | ""
    | null -> printfn "%s" j
    | _ -> File.WriteAllText(path, j)

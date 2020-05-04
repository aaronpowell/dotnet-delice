[<AutoOpen>]
module Output

open LicenseBuilder
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Spdx

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type Package =
    { Name: string
      Version: string
      Url: string Option
      DisplayName: string }

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type PrintableLicense =
    { Expression: string
      Count: int
      Packages: Package seq
      IsOsi: bool
      IsFsf: bool
      IsDeprecatedType: bool }

// hacky little function I use to code gen the cache
let private licensesCodeGen legacyLicensed =
    legacyLicensed
    |> Seq.groupBy (fun l -> l.Url)
    |> Seq.iter (fun (url, pkgs) ->
        printfn "(\"%s\", { Expression = \"\"; Packages = Map.ofList[%s]})" url.Value
            (pkgs
             |> Seq.map (fun p -> sprintf "(\"%s\", [\"%A\"])" p.Name p.Version)
             |> String.concat "; "))

let getSpdxInfo licenseId =
    async {
        let! spdx = getSpdx false
        match spdx.Licenses
              |> Array.tryFind (fun l -> l.LicenseId = licenseId) with
        | Some spdxInfo ->
            return (spdxInfo.IsOsiApproved,
                    (match spdxInfo.IsFsfLibre with
                     | Some b -> b
                     | None -> false),
                    spdxInfo.IsDeprecatedLicenseId)
        | None -> return (false, false, false)
    }
    |> Async.RunSynchronously

let getProjectBreakdown licenses =
    let unlicensed =
        licenses
        |> Seq.choose (fun l ->
            match l with
            | PackageNotFound l -> Some l
            | _ -> None)
        |> Seq.filter (fun l -> l.Type = "package")
        |> Seq.sortBy (fun l -> l.PackageName)

    let projectReferences =
        licenses
        |> Seq.choose (fun l ->
            match l with
            | PackageNotFound l -> Some l
            | _ -> None)
        |> Seq.filter (fun l -> l.Type = "project")
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

    (unlicensed, projectReferences, licensed, legacyLicensed)

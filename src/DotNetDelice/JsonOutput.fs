[<AutoOpen>]
module JsonOutput

open NuGet.ProjectModel
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open OptionConverter
open System.IO

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type PackageOutput =
    { ProjectName: string
      Licenses: PrintableLicense seq }

let jsonBuilder name licenses =
    let unlicensed, projectReferences, licensed, legacyLicensed = getProjectBreakdown licenses

    let pl =
        Seq.append
            (if Seq.length unlicensed > 0 then
                [| { Expression = "Missing"
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
                     IsDeprecatedType = false } |]
             else
                 [||])
            (if Seq.length legacyLicensed > 0 then
                [| { Expression = "Unable to determine"
                     Count = Seq.length legacyLicensed
                     Packages =
                         legacyLicensed
                         |> Seq.map
                             (fun l ->
                             { Name = l.PackageName
                               Version = l.PackageVersion.OriginalVersion
                               Url = Some l.Url
                               DisplayName = sprintf "%s@%s (%s)" l.PackageName l.PackageVersion.OriginalVersion l.Url })
                     IsOsi = false
                     IsFsf = false
                     IsDeprecatedType = false } |]
             else
                 [||])
        |> Seq.append
            (if Seq.length projectReferences > 0 then
                [| { Expression = "Project References"
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
                     IsDeprecatedType = false } |]
             else
                 [||])

    { ProjectName = name
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
                Packages =
                    packages
                    |> Seq.map (fun l ->
                        { Name = l.PackageName
                          Version = l.PackageVersion.OriginalVersion
                          Url = Some l.Url
                          DisplayName = sprintf "%s@%s" l.PackageName l.PackageVersion.OriginalVersion })
                IsOsi = osi
                IsFsf = fsf
                IsDeprecatedType = dep })
          |> Seq.append pl }

[<JsonObjectAttribute(NamingStrategyType = typeof<CamelCaseNamingStrategy>)>]
type ProjectOutput =
    { Projects: PackageOutput seq }

let jsonPrinter path json =
    let settings = JsonSerializerSettings()
    settings.Converters.Add(OptionConverter())
    let j = JsonConvert.SerializeObject({ Projects = json }, Formatting.Indented, settings)
    match path with
    | ""
    | null -> printfn "%s" j
    | _ -> File.WriteAllText(path, j)

module LicenseBuilder

open NuGet.Versioning
open System
open NuGet.Protocol
open NuGet.Packaging.Core
open NuGet.ProjectModel
open LicenseCache

type LicenseMetadata =
    { Type : string option
      Version : Version option
      Url : string
      PackageName : string
      PackageVersion : NuGetVersion }

type MissingLicense =
    { PackageName : string
      PackageVersion : NuGetVersion }

type LicenseResult =
    | Licensed of LicenseMetadata
    | LegacyLicensed of LicenseMetadata
    | PackageNotFound of MissingLicense

let rec private findPackage paths identity logger =
    match paths with
    | head :: rest ->
        match LocalFolderUtility.GetPackageV3(head, identity, logger) with
        | null -> findPackage rest identity logger
        | pkg -> Some pkg
    | [] -> None

let getPackageLicense (projectSpec : PackageSpec) (lib : LockFileLibrary) =
    let identity = PackageIdentity(lib.Name, lib.Version)

    let nugetPaths =
        [| projectSpec.RestoreMetadata.PackagesPath |]
        |> Seq.append projectSpec.RestoreMetadata.FallbackFolders
        |> Seq.toList
    match findPackage nugetPaths identity MemoryLogger.Instance with
    | None ->
        { PackageName = lib.Name
          PackageVersion = lib.Version }
        |> PackageNotFound
    | Some pId ->
        let licenseMetadata =
            { Type = None
              Version = None
              Url = pId.Nuspec.GetLicenseUrl()
              PackageName = lib.Name
              PackageVersion = lib.Version }
        match pId.Nuspec.GetLicenseMetadata() with
        | null ->
            match knownLicenseCache.TryFind <| lib.Name.ToLower() with
            | Some cachedPkg ->
                match cachedPkg.TryFind <| lib.Version.ToString() with
                | Some v ->
                    { licenseMetadata with Type = Some v
                                           Version = None }
                    |> Licensed
                | None -> licenseMetadata |> LegacyLicensed
            | None -> licenseMetadata |> LegacyLicensed
        | licence ->
            { licenseMetadata with Type = Some licence.License
                                   Version = Some licence.Version }
            |> Licensed

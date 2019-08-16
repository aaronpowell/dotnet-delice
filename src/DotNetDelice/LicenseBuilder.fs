module LicenseBuilder

open NuGet.Versioning
open System
open NuGet.Protocol
open NuGet.Packaging.Core
open NuGet.ProjectModel

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
    | Unlicensed of MissingLicense

let getPackageLicense (projectSpec : PackageSpec) (lib : LockFileLibrary) =
    let identity = PackageIdentity(lib.Name, lib.Version)
    let pId =
        LocalFolderUtility.GetPackageV3
            (projectSpec.RestoreMetadata.PackagesPath, identity, NuGet.Common.NullLogger.Instance)
    match pId with
    | null ->
        { PackageName = lib.Name
          PackageVersion = lib.Version }
        |> Unlicensed
    | _ ->
        match pId.Nuspec.GetLicenseMetadata() with
        | null ->
            { Type = None
              Version = None
              Url = pId.Nuspec.GetLicenseUrl()
              PackageName = lib.Name
              PackageVersion = lib.Version }
            |> LegacyLicensed
        | licence ->
            { Type = Some licence.License
              Version = Some licence.Version
              Url = licence.LicenseUrl.ToString()
              PackageName = lib.Name
              PackageVersion = lib.Version }
            |> Licensed

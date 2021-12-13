module LicenseBuilder

open NuGet.Versioning
open System
open NuGet.Protocol
open NuGet.Packaging.Core
open NuGet.ProjectModel
open LicenseCache
open NuGet.Packaging
open System.IO
open Spdx
open System.Linq
open System.Text.RegularExpressions

type LicenseMetadata =
    { Type: string option
      Version: Version option
      Url: string
      PackageName: string
      PackageVersion: NuGetVersion }

type MissingLicense =
    { PackageName: string
      PackageVersion: NuGetVersion
      Type: string }

type LicenseResult =
    | Licensed of LicenseMetadata
    | LegacyLicensed of LicenseMetadata
    | PackageNotFound of MissingLicense

let rec private findPackage paths identity logger =
    match paths with
    | head :: rest ->
        match LocalFolderUtility.GetPackageV3(head, identity, logger) with
        | null -> findPackage rest identity logger
        | pkg -> Some(pkg, head)
    | [] -> None

let private trimUrl (url: string) =
    let licenseUrlRegexResult =
        Regex.Match(url, "^https?(?<trimmedUrl>:\/\/.*?)(?:\.(?:html?|txt))?$", RegexOptions.IgnoreCase)

    if licenseUrlRegexResult.Success then
        licenseUrlRegexResult.Groups.["trimmedUrl"].Value
    else
        url

let private getSpdxLicenseByAlternateUrl licenseUrl =
    async {
        if isNull licenseUrl then
            return None
        else
            let! spdx = getSpdx false
            let trimmedUrl = trimUrl licenseUrl

            match spdx.Licenses
                  |> Array.tryFind (fun l ->
                      l.SeeAlso.Any(fun sa -> sa.Contains(trimmedUrl, StringComparison.OrdinalIgnoreCase)))
                with
            | Some spdxInfo -> return Some spdxInfo.LicenseId
            | None -> return None
    }
    |> Async.RunSynchronously

let private buildLicenseFromPackage
    checkGitHub
    token
    checkLicenseContents'
    (identity: PackageIdentity)
    packagePath
    (pId: LocalPackageInfo)
    path
    =
    let licenseMetadata =
        { Type = None
          Version = None
          Url = pId.Nuspec.GetLicenseUrl()
          PackageName = identity.Id
          PackageVersion = identity.Version }

    match pId.Nuspec.GetLicenseMetadata() with
    | null ->
        let url = pId.Nuspec.GetLicenseUrl()
        let projectUrl = pId.Nuspec.GetProjectUrl()

        match getSpdxLicenseByAlternateUrl url with
        | None ->
            match checkGitHub, knownLicenseCache.TryFind url with
            | (_, Some cachedLicense) ->
                { licenseMetadata with
                    Type = Some cachedLicense.Expression
                    Version = None }
                |> Licensed
            | (true, None) ->
                match checkProjectAndLicenseViaGitHub token url projectUrl with
                | Some cachedLicense ->
                    { licenseMetadata with
                        Type = Some cachedLicense.Expression
                        Version = None }
                    |> Licensed
                | None ->
                    match checkLicenseContents' identity.Id url with
                    | Some cachedLicense ->
                        { licenseMetadata with
                            Type = Some cachedLicense.Expression
                            Version = None }
                        |> Licensed
                    | None -> licenseMetadata |> LegacyLicensed
            | (false, None) ->
                match checkLicenseContents' identity.Id url with
                | Some cachedLicense ->
                    { licenseMetadata with
                        Type = Some cachedLicense.Expression
                        Version = None }
                    |> Licensed
                | None -> licenseMetadata |> LegacyLicensed
        | Some license ->
            { licenseMetadata with
                Type = Some license
                Version = None }
            |> Licensed
    | license when license.Type = LicenseType.File ->
        match Path.Combine(path, packagePath, license.License.Replace('\\', Path.DirectorySeparatorChar))
              |> File.ReadAllText
              |> findMatchingLicense
            with
        | Some licenseSpdx ->
            { licenseMetadata with
                Type = Some licenseSpdx
                Version = Some license.Version }
            |> Licensed
        | None ->
            { licenseMetadata with
                Type = Some license.License
                Version = Some license.Version }
            |> Licensed
    | license ->
        { licenseMetadata with
            Type = Some license.License
            Version = Some license.Version }
        |> Licensed

let getPackageLicense
    (projectSpec: PackageSpec)
    checkGitHub
    token
    checkLicenseContent
    packageName
    packageVersion
    packageType
    =
    let identity =
        PackageIdentity(packageName, packageVersion)

    let checkLicenseContents' name url =
        if checkLicenseContent then
            checkLicenseContents name url
        else
            None

    let nugetPaths =
        [| projectSpec.RestoreMetadata.PackagesPath |]
        |> Seq.append projectSpec.RestoreMetadata.FallbackFolders
        |> Seq.toList

    match findPackage nugetPaths identity MemoryLogger.Instance with
    | None ->
        { PackageName = identity.Id
          PackageVersion = identity.Version
          Type = packageType }
        |> PackageNotFound
    | Some (pId, path) ->
        buildLicenseFromPackage
            checkGitHub
            token
            checkLicenseContents'
            identity
            ((sprintf "%s/%A" identity.Id identity.Version)
                .ToLower())
            pId
            path

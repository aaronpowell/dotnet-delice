module LicenseCache

open System.Text.RegularExpressions

// This is a lookup against a number of standard projects and their license type.
// The NuGet versions here use the legacy license format, so the URL is the only
// thing that will tell you what the license is. This cache means we don't have
// to query the URL each run.
type LicenseCache =
    { Expression : string
      Packages : Map<string, string list> }

let knownLicenseCache =
    Map.ofList [ ("https://github.com/Microsoft/visualfsharp/blob/master/License.txt",
                  { Expression = "MIT"
                    Packages = Map.ofList [ ("fsharp.core", [ "4.5.0"; "4.5.1"; "4.5.2" ]) ] })
                 ("http://go.microsoft.com/fwlink/?LinkId=329770",
                  { Expression = "Microsoft Software License"
                    Packages =
                        Map.ofList [ ("microsoft.csharp", [ "4.0.1" ])
                                     ("microsoft.win32.primitives", [ "4.3.0" ])
                                     ("microsoft.win32.registry", [ "4.3.0" ])
                                     ("runtime.native.system", [ "4.3.0" ])
                                     ("system.collections", [ "4.3.0" ])
                                     ("system.diagnostics.debug", [ "4.3.0" ])
                                     ("system.diagnostics.process", [ "4.3.0" ])
                                     ("system.diagnostics.tools", [ "4.0.1" ])
                                     ("system.dynamic.runtime", [ "4.3.0" ])
                                     ("system.globalization", [ "4.3.0" ])
                                     ("system.io", [ "4.3.0" ])
                                     ("system.io.filesystem", [ "4.3.0" ])
                                     ("system.io.filesystem.primitives", [ "4.3.0" ])
                                     ("system.linq", [ "4.3.0" ])
                                     ("system.linq.expressions", [ "4.3.0" ])
                                     ("system.objectmodel", [ "4.3.0" ])
                                     ("system.reflection", [ "4.3.0" ])
                                     ("system.reflection.emit", [ "4.3.0" ])
                                     ("system.reflection.emit.ilgeneration", [ "4.3.0" ])
                                     ("system.reflection.emit.lightweight", [ "4.3.0" ])
                                     ("system.reflection.extensions", [ "4.3.0" ])
                                     ("system.reflection.primitives", [ "4.3.0" ])
                                     ("system.reflection.typeextensions", [ "4.3.0" ])
                                     ("system.resources.resourcemanager", [ "4.3.0" ])
                                     ("system.runtime", [ "4.3.0" ])
                                     ("system.runtime.extensions", [ "4.3.0" ])
                                     ("system.runtime.handles", [ "4.3.0" ])
                                     ("system.runtime.interopservices", [ "4.3.0" ])
                                     ("system.runtime.serialization.primitives", [ "4.1.1" ])
                                     ("system.security.cryptography.primitives", [ "4.3.0" ])
                                     ("system.security.cryptography.protecteddata", [ "4.3.0" ])
                                     ("system.text.encoding", [ "4.3.0" ])
                                     ("system.text.encoding.extensions", [ "4.3.0" ])
                                     ("system.text.regularexpressions", [ "4.1.0" ])
                                     ("system.threading", [ "4.3.0" ])
                                     ("system.threading.tasks", [ "4.3.0" ])
                                     ("system.threading.tasks.extensions", [ "4.0.0" ])
                                     ("system.threading.thread", [ "4.3.0" ])
                                     ("system.threading.threadpool", [ "4.3.0" ])
                                     ("system.xml.readerwriter", [ "4.0.11" ])
                                     ("system.xml.xdocument", [ "4.0.11" ]) ] })
                 ("https://github.com/dotnet/core-setup/blob/master/LICENSE.TXT",
                  { Expression = "MIT"
                    Packages =
                        Map.ofList [ ("Microsoft.NETCore.App", [ "2.2.0" ])
                                     ("Microsoft.NETCore.DotNetAppHost", [ "2.2.0" ])
                                     ("Microsoft.NETCore.DotNetHostPolicy", [ "2.2.0" ])
                                     ("Microsoft.NETCore.DotNetHostResolver", [ "2.2.0" ]) ] })
                 ("https://github.com/dotnet/corefx/blob/master/LICENSE.TXT",
                  { Expression = "MIT"
                    Packages =
                        Map.ofList [ ("Microsoft.NETCore.Platforms", [ "2.2.0" ])
                                     ("Microsoft.NETCore.Targets", [ "2.0.0" ])
                                     ("System.ComponentModel.Annotations", [ "4.4.1" ]) ] })
                 ("https://github.com/dotnet/standard/blob/master/LICENSE.TXT",
                  { Expression = "MIT"
                    Packages = Map.ofList [ ("NETStandard.Library", [ "2.0.3" ]) ] })
                 ("https://raw.github.com/JamesNK/Newtonsoft.Json/master/LICENSE.md",
                  { Expression = "MIT"
                    Packages = Map.ofList [ ("Newtonsoft.Json", [ "9.0.1" ]) ] })
                 ("https://raw.githubusercontent.com/aspnet/AspNetCore/2.0.0/LICENSE.txt",
                  { Expression = "Apache-2.0"
                    Packages =
                        Map.ofList [ ("Microsoft.AspNetCore", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Antiforgery", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authentication", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authentication.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authentication.Cookies", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authentication.Core", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authentication.JwtBearer", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authentication.OAuth", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Authorization", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Connections.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Cors", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Cryptography.Internal", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.DataProtection", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.DataProtection.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Diagnostics", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Diagnostics.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.HostFiltering", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Hosting", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Hosting.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Hosting.Server.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Http", [ "2.2.2" ])
                                     ("Microsoft.AspNetCore.Http.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Http.Extensions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Http.Features", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.HttpOverrides", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.ResponseCaching", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.ResponseCaching.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.ResponseCompression", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Rewrite", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Routing", [ "2.2.2" ])
                                     ("Microsoft.AspNetCore.Routing.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Server.IIS", [ "2.2.2" ])
                                     ("Microsoft.AspNetCore.Server.IISIntegration", [ "2.2.1" ])
                                     ("Microsoft.AspNetCore.Server.Kestrel", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Server.Kestrel.Core", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Server.Kestrel.Https", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets", [ "2.2.1" ])
                                     ("Microsoft.AspNetCore.Session", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.StaticFiles", [ "2.2.0" ])
                                     ("Microsoft.AspNetCore.WebUtilities", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Caching.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Caching.Memory", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Configuration", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Configuration.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Configuration.Binder", [ "2.2.4" ])
                                     ("Microsoft.Extensions.Configuration.CommandLine", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Configuration.EnvironmentVariables", [ "2.2.4" ])
                                     ("Microsoft.Extensions.Configuration.FileExtensions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Configuration.Json", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Configuration.UserSecrets", [ "2.2.0" ])
                                     ("Microsoft.Extensions.DependencyInjection", [ "2.2.0" ])
                                     ("Microsoft.Extensions.DependencyInjection.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.FileProviders.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.FileProviders.Physical", [ "2.2.0" ])
                                     ("Microsoft.Extensions.FileSystemGlobbing", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Hosting.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Logging", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Logging.Abstractions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Logging.Configuration", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Logging.Console", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Logging.Debug", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Logging.EventSource", [ "2.2.0" ])
                                     ("Microsoft.Extensions.ObjectPool", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Options", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Options.ConfigurationExtensions", [ "2.2.0" ])
                                     ("Microsoft.Extensions.Primitives", [ "2.2.0" ])
                                     ("Microsoft.Extensions.WebEncoders", [ "2.2.0" ])
                                     ("Microsoft.Net.Http.Headers", [ "2.2.0" ]) ] })
                 ("https://go.microsoft.com/fwlink/?linkid=2028464",
                  { Expression = "Microsoft Software License"
                    Packages =
                        Map.ofList [ ("Microsoft.Azure.WebJobs", [ "3.0.6" ])
                                     ("Microsoft.Azure.WebJobs.Core", [ "3.0.6" ])
                                     ("Microsoft.Azure.WebJobs.Extensions", [ "3.0.2" ])
                                     ("Microsoft.Azure.WebJobs.Extensions.EventHubs", [ "3.0.4" ])
                                     ("Microsoft.Azure.WebJobs.Extensions.Http", [ "3.0.2" ])
                                     ("Microsoft.Azure.WebJobs.Extensions.Storage", [ "3.0.5" ])
                                     ("Microsoft.Azure.WebJobs.Host.Storage", [ "3.0.6" ])
                                     ("Microsoft.Azure.WebJobs.Script.ExtensionsMetadataGenerator", [ "1.1.0" ]) ] }) ]

open FSharp.Data

type LicenseResponse = JsonProvider<""" { "license": { "key": "", "name": "", "spdx_id": "" } } """>

let mutable private dynamicLicenseCache = Map.empty<string, LicenseCache>
let mutable private githubNoAssertion = List.empty<string>

let (|CompiledMatch|_|) pattern input =
    if isNull input then None
    else
        let m = Regex.Match(input, pattern, RegexOptions.Compiled)
        if m.Success then
            Some [ for x in m.Groups -> x ]
        else None

let checkLicenseViaGitHub token licenseUrl =
    match dynamicLicenseCache.TryFind licenseUrl with
    | Some cache -> Some cache
    | None ->
        if githubNoAssertion |> List.contains licenseUrl then None
        else
            match licenseUrl with
            | CompiledMatch ".*(github|githubusercontent)\.com\/([\w\-]*)\/([\w\-_\.]*)\/.*"
              [ _; _; orgGroup; repoGroup ] ->
                let org = orgGroup.Value
                let repo = repoGroup.Value

                let headers =
                    [ "User-Agent", "dotnet-delice" ]
                    |> List.append (match token with
                                    | null
                                    | "" -> []
                                    | _ -> [ "Authorization", sprintf "token %s" token ])

                let res =
                    Http.RequestString(sprintf "https://api.github.com/repos/%s/%s/license" org repo, headers = headers)
                    |> LicenseResponse.Parse
                if res.License.SpdxId.JsonValue.AsString() = "NOASSERTION" then
                    // this is what the GitHub API returns if it can't determine the license of a repository
                    githubNoAssertion <- licenseUrl :: githubNoAssertion
                    None
                else
                    let lc =
                        { Expression = res.License.SpdxId.JsonValue.AsString()
                          Packages = Map.ofList [ (repo, []) ] }
                    dynamicLicenseCache <- dynamicLicenseCache.Add(licenseUrl, lc)
                    Some lc
            | _ -> None

open CommonLicenseDescriptions

// adapted from https://lucidjargon.wordpress.com/2010/11/29/dices-coefficient-in-f/
let bigrams (s : string) =
    s.ToLower().ToCharArray().[0..s.Length - 2]
    |> Array.fold (fun (set : Set<string>, i) c -> set.Add(c.ToString() + s.[i + 1].ToString()), i + 1) (Set.empty, 0)
    |> fst

let diceCoefficient (a : string) (b : string) =
    let f str = str |> Array.fold (fun fset arr -> Set.union fset (bigrams arr)) Set.empty
    let s1 = a.Split(' ') |> f
    let s2 = b.Split(' ') |> f
    2. * float ((Set.intersect s1 s2).Count) / float (s1.Count + s2.Count)

let mutable private failedUrls = List.empty<string>

let private convertIfGitHub (licenseUrl : string) =
    if Regex.IsMatch(licenseUrl, "^https?:\/\/github\.com") then
        Regex.Replace(licenseUrl, "^https?:\/\/github\.com", "https://raw.githubusercontent.com").Replace("/blob/", "/")
    else licenseUrl

let findMatchingLicense licenseContents =
    descriptions |> Map.tryFindKey (fun _ licenseTemplate -> diceCoefficient licenseTemplate licenseContents > 0.98)

let checkLicenseContents packageName licenseUrl =
    match dynamicLicenseCache.TryFind licenseUrl with
    | Some lc -> Some lc
    | None ->
        if failedUrls |> List.contains licenseUrl then None
        else
            try
                let licenseContents = convertIfGitHub licenseUrl |> Http.RequestString
                match findMatchingLicense licenseContents with
                | Some key ->
                    let lc =
                        { Expression = key
                          Packages = Map.ofList [ (packageName, []) ] }
                    dynamicLicenseCache <- dynamicLicenseCache.Add(licenseUrl, lc)
                    Some lc
                | None ->
                    failedUrls <- licenseUrl :: failedUrls
                    None
            with _ ->
                failedUrls <- licenseUrl :: failedUrls
                None

[![Build Status](https://dev.azure.com/aaronpowell/dotnet-delice/_apis/build/status/master%20build?branchName=master)](https://dev.azure.com/aaronpowell/dotnet-delice/_build/latest?definitionId=29&branchName=master) [![NuGet Badge](https://buildstats.info/nuget/dotnet-delice)](https://www.nuget.org/packages/dotnet-delice) [![The MIT License](https://img.shields.io/badge/license-MIT-orange.svg?color=blue&style=flat-square)](http://opensource.org/licenses/MIT)

# dotnet-delice

delice is a tool for determining the license information of the packages that are referenced in a project/solution. This is a port of the Node.js utility [`delice`](https://github.com/cutenode/delice), created by [Tierney Cyren](https://github.com/bnb).

**Note**: `dotnet-delice` only supports SDK project files for C#, F# and VB.NET (although I'm not sure on VB.NET, never tried it!), not the legacy "MSBuild style" project files (which only support .NET full framework). If you are still using the legacy project file the tool will fail. I'd encourage you to try and upgrade (using a tool such as [CsprojToVs2017](https://github.com/hvanbakel/CsprojToVs2017)).

# Usage

This tool ships as a [`dotnet` global tool](https://docs.microsoft.com/dotnet/core/tools/global-tools?WT.mc_id=javascript-0000-aapowell) and can be installed like so:

```
dotnet tool install -g dotnet-delice
```

You can then use it like so:

```
dotnet delice [folder, sln, csproj, fsproj]
```

## Commands

- `-?|-h|--help` Boolean. Show help.
- `-j|--json` Boolean. Output results as JSON rather than pretty-print.
- `--json-output [path]` String. Path to file that the JSON should be written to. Note: Only in use if you use `-j|--json`.
- `--check-github` Boolean. If the license URL (for a legacy package) points to a GitHub hosted file, use the GitHub API to try and retrieve the license type.
- `--github-token <token>` String. A GitHub Personal Access Token (PAT) to use when checking the GitHub API for license types. This avoids being [rate limited](https://developer.github.com/v3/#rate-limiting) when checking a project.
- `--check-license-content` Boolean. When provided the contents of the license file will be compared to known templates.
- `--refresh-spdx` Boolean. When provided the tool will also refresh the SPDX license cache used for conformance infomation.

## Output

- Project Name
  - The name of the project that was checked
- License Expression
  - A license expression found when parsing references
  - Some packages may result in an undetermined license. See [Undetermined Licenses](#undetermined-licenses) for more information
- Packages
  - The name(s) of the packages found for that license

The following is an example of pretty-printed output:

```
Project dotnet-delice
License Expression: MIT
├── There are 10 occurances of MIT
├─┬ Conformance:
│ ├── Is OSI Approved: true
│ ├── Is FSF Free/Libre: true
│ └── Included deprecated IDs: false
└─┬ Packages:
  ├── FSharp.Core
  ├── Microsoft.NETCore.App
  ├── Microsoft.NETCore.DotNetAppHost
  ├── Microsoft.NETCore.DotNetHostPolicy
  ├── Microsoft.NETCore.DotNetHostResolver
  ├── Microsoft.NETCore.Platforms
  ├── Microsoft.NETCore.Targets
  ├── NETStandard.Library
  ├── Newtonsoft.Json
  └── System.ComponentModel.Annotations
```

# Roadmap

- Ability to filter for only a particular license
- Anything you'd like? Open an [issue](https://github.com/aaronpowell/dotnet-delice/issues) 😁

# Undetermined Licenses

At the end of 2018 the [`licenseUrl` field in the nuspec file was deprecated](https://github.com/NuGet/Announcements/issues/32) to be replaced with a richer license metadata field. You can read more about it in the [annuncement](https://github.com/NuGet/Announcements/issues/32), the [documentation](https://docs.microsoft.com/nuget/reference/nuspec?WT.mc_id=javascript-0000-aapowell#license) and [Spec wiki](https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg).

This new metadata makes it possible to determine from the package what the license in use by a package is, rather than relying on navigating through to the referred license file.

Some NuGet packages have moved over to the new format, but many of them are still using the legacy approach which makes it difficult for delice to determine what the license is of a package.

By default these packages will be reported with an "Unable to determine" license type with the URL of the license URL included in the output but there are two options that can be set at the CLI to help attempt to discover what the license is.

## Using GitHub's API to Check Licenses

Projects hosted on GitHub will often have their license shown on the repository header, which is done by GitHub scanning the license file in the repository and determine the appropriate type. This can be accessed via [GitHub's API](https://developer.github.com/v3/licenses/#get-the-contents-of-a-repositorys-license) and `delice` provides an integration to it.

When the `--check-github` flag is set `delice` will check if the projects license URL points to a GitHub-hosted file, if it does, it'll attempt to get the owner and repo name from the URL to then call the GitHub API. If the API returns a detected license the license information will be updated in the response from `delice`.

It's recommended to also use the `--github-token <token>` CLI option to provide a GitHub Personal Access Token to authenticate the requests (they are anonymous by default) as this will avoid rate-limiting happening with the API.

## Checking License Contents

GitHub uses [Licensee](https://licensee.github.io/licensee/) in its [detecting a license](https://help.github.com/en/articles/licensing-a-repository#detecting-a-license). Licensee will look at the contents of the license and compare it to license templates using [Sørensen–Dice coefficient](https://en.wikipedia.org/wiki/S%C3%B8rensen%E2%80%93Dice_coefficient).

`delice` also supports doing this via the `--check-license-contents` flag. When provided `delice` will download the contents of the `licenseUrl` in the nuspec and compare it to known templates stored within itself. The comparison requires that the license and template be _at least_ 90% the same for it to be considered a match (this is lower than Licensee, which uses 98%, but experiments against .NET showed it was better to be a bit looser), so there is still some potential misses.

Also, only certain license templates are stored within `delice`, but feel free to add more via PR's.

This can work in conjunction with the GitHub API test, but will be run _after_ the API check is done, and only if it fails.

## Common License Cache

The file [`LicenseCache.fs`](src/DotNetDelice.Licensing/LicenseCache.fs) contains a map of commonly used packages and the license file that they have. This means that delice can determine more licenses out of the box.

If you're coming across packages that you think should be in there, open a Pull Request with the updates.

# Related Projects

This project is a port of the Node.js utility [`delice`](https://github.com/cutenode/delice), created by [Tierney Cyren](https://github.com/bnb) and aims to provide the same sorts of functionality but in a .NET friendly workflow.

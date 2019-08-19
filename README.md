[![Build Status](https://dev.azure.com/aaronpowell/dotnet-delice/_apis/build/status/master%20build?branchName=master)](https://dev.azure.com/aaronpowell/dotnet-delice/_build/latest?definitionId=29&branchName=master) [![NuGet Badge](https://buildstats.info/nuget/dotnet-delice)](https://www.nuget.org/packages/dotnet-delice) [![The MIT License](https://img.shields.io/badge/license-MIT-orange.svg?color=blue&style=flat-square)](http://opensource.org/licenses/MIT)

# dotnet-delice

delice is a tool for determining the license information of the packages that are referenced in a project/solution. This is a port of the Node.js utility [`delice`](https://github.com/cutenode/delice), created by [Tierney Cyren](https://github.com/bnb).

# Usage

This tool ships as a [`dotnet` global tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools?WT.mc_id=dotnetdelice-github-aapowell) and can be installed like so:

```
dotnet tool install -g dotnet-delice
```

You can then use it like so:

```
dotnet delice [folder, sln, csproj, fsproj]
```

## Commands

- `-?|-h|--help` Boolean. Show help
- `-j|--json` Boolean. Output results as JSON rather than pretty-print
- `--json-output [path]` String. Path to file that the JSON should be written to. Note: Only in use if you use `-j|--json`.

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

- Attempt to determine license from license file
  - Use the GitHub API (if a GitHub repo) or basic file parsing to detect license info (see [undetermined licenses](#undetermined-licenses))
- Show conformance info from the licenses
- Ability to filter for only a particular license
- Anything you'd like? Open an [issue](https://github.com/aaronpowell/dotnet-delice/issues) 😁

# Undetermined Licenses

At the end of 2018 the [`licenseUrl` field in the nuspec file was deprecated](https://github.com/NuGet/Announcements/issues/32) to be replaced with a richer license metadata field. You can read more about it in the [annuncement](https://github.com/NuGet/Announcements/issues/32), the [documentation](https://docs.microsoft.com/en-us/nuget/reference/nuspec?WT.mc_id=dotnetdelice-github-aapowell#license) and [Spec wiki](https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg).

This new metadata makes it possible to determine from the package what the license in use by a package is, rather than relying on navigating through to the referred license file.

Some NuGet packages have moved over to the new format, but many of them are still using the legacy approach which makes it difficult for delice to determine what the license is of a package. Presently, these packages will be reported with an "Unable to determine" license type with the URL of the license URL included in the output.

## Common License Cache

The file [`LicenseCache.fs`](blob/master/src/DotNetDelice/LicenseCache.fs) contains a map of commonly used packages and the license file that they have. This means that delice can determine more licenses out of the box.

If you're coming across packages that you think should be in there, open a Pull Request with the updates.

# Related Projects

This project is a port of the Node.js utility [`delice`](https://github.com/cutenode/delice), created by [Tierney Cyren](https://github.com/bnb) and aims to provide the same sorts of functionality but in a .NET friendly workflow.

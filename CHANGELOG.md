# Changelog for `dotnet-delice`

## [1.7.1] - 2022-05-04

### Changed

- Merged PR #31 to fix readme
- Fixed #32

## [1.7.0] - 2021-12-14

### Changed

- Updated to .NET 6
- Merged PR #30 to better support multi-license packages

## [1.6.0] - 2021-03-30

### Changed

- Updated to .NET 5
- Upgraded packages
- Added a devcontainer for easier development
- Moved to GitHub Actions and created new build/release pipeline

## [1.5.2] - 2020-06-09

### Fixed

- Error handling the potential for a failed response when querying GitHub for license info (issue #21)

## [1.5.1] - 2020-06-09

### Fixed

- Normalising paths to license within a nuget package across OSes (issue #20)

### Changed

- Moving the common licenses to separate files rather than one big one

## [1.5.0] - 2020-05-04

### Changed

- Detecting dotnet tool references and handling them (issue #15)
- Making `isFsfLibre` optional since it's often missing in the JSON SPDX response (issue #14)

## [1.4.0] - 2020-01-10

### Changed

- Detecting unknown project styles and excluding them to reduce likelyhood of crashes when running against full framework projects

## [1.3.0] - 2019-10-24

### Added

- Putting version number for package in properties of the JSON output (issues #7)
- Putting URL for package license in properties of the JSON output (issues #9)

### Changed

- JSON output now includes the version and URL for packages (where available)
- Pretty print output includes version
- Better error message when the dependency graph for a project fails to load

## [1.2.0] - 2019-10-23

### Added

- Detection of Project References as a unique license mode

### Changed

- Project References are now extracted out to a separate node in the response since their license is unknown
- Updated to .NET Core 3.0

## [1.1.0] - 2019-08-28

### Added

- Support for looking up a license via the GitHub API when the license is hosted on GitHub
  - This reduces the number of packages that return "Unable to determine"
  - User is able to provide a GitHub Personal Access Token (PAT) to avoid being rate-limited
- Support for looking at the contents of the license file to see if we can guess the type
- Showing license conformance with SPDX info (whether it's OSI, FSF, etc.). This is obtained from the SDPX database, which can be refreshed using a CLI flag

### Changed

- Extracted the core of the project out to a separate project so it can be its own NuGet package
- Output now contains a new node for conformance in the pretty-print, and three new nodes in JSON, `isOsi`, `isFsf` and `isDeprecatedType`

## [1.0.0] - 2019-08-19

Initial Release :tada:

### Added

- Locate all dependencies for a project and find their licenses
- Packages using the [legacy `licenseUrl` field](https://github.com/NuGet/Announcements/issues/32) don't have their license properly discovered
- Some packages using the legacy format are pre-parsed and stored in code
- Display output as pretty console output _or_ JSON
- Ability to save JSON to file

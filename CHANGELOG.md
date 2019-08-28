# Changelog for `dotnet-delice`

## [Unreleased]

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

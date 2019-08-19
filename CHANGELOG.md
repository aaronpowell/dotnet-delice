# Changelog for `dotnet-delice`

## [1.0.0] - 2019-08-19

Initial Release :tada:

### Added

- Locate all dependencies for a project and find their licenses
- Packages using the [legacy `licenseUrl` field](https://github.com/NuGet/Announcements/issues/32) don't have their license properly discovered
- Some packages using the legacy format are pre-parsed and stored in code

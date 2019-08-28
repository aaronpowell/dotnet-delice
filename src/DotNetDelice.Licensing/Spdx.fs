module Spdx

open FSharp.Data
open System.IO

[<Literal>]
let private URL = "https://spdx.org/licenses/licenses.json"

type Spdx = JsonProvider<"""{ "licenses": [{
      "reference": "./MIT.html",
      "isDeprecatedLicenseId": false,
      "isFsfLibre": true,
      "detailsUrl": "http://spdx.org/licenses/MIT.json",
      "referenceNumber": "201",
      "name": "MIT License",
      "licenseId": "MIT",
      "seeAlso": [
        "https://opensource.org/licenses/MIT"
      ],
      "isOsiApproved": true
    }] }""">

let private download() = Spdx.AsyncLoad URL
let private getFileLocation() = Path.Combine(System.Environment.CurrentDirectory, "licenses.json")

let getSpdx refreshFile =
    async {
        if refreshFile then
            if File.Exists(getFileLocation()) then File.Delete <| getFileLocation()
            let! contents = download()
            File.WriteAllText(getFileLocation(), contents.ToString())
            return contents
        else
            if not (File.Exists(getFileLocation())) then
                let! contents = download()
                File.WriteAllText(getFileLocation(), contents.ToString())
                return contents
            else
                let contents = File.ReadAllText <| getFileLocation()
                return Spdx.Parse contents
    }


module CommonLicenseDescriptions

open MIT
open Apache
open CPL
open GPL
open BSD3
open NetFoundation
open MSLicense

type Description =
    { Expression: string
      Template: string }

let descriptions = Map.ofList [ mit; apache; cpl; gpl_v2; bsd3; netfoundation; mslicense; ]

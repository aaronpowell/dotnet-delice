module CommonLicenseDescriptions

open MIT
open Apache
open CPL
open GPL

type Description =
    { Expression: string
      Template: string }

let descriptions = Map.ofList [ mit; apache; cpl; gpl_v2 ]

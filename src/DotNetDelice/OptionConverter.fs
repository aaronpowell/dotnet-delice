module OptionConverter

open System
open Microsoft.FSharp.Reflection

open Newtonsoft.Json

// Adapted from https://github.com/haf/Newtonsoft.Json.FSharp/blob/a62fc940f30369592a8c2f9fdaf0ed6bf38ef9ee/src/JsonNet/Converters/OptionConverter.fs
/// F# options-converter
type OptionConverter() =
  inherit JsonConverter()

  override x.CanConvert t =
    t.IsGenericType
    && typedefof<option<_>>.Equals (t.GetGenericTypeDefinition())

  override x.WriteJson(writer, value, serializer) =
    let value =
      if isNull value then
        null
      else
        let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
        fields.[0]
    serializer.Serialize(writer, value)

  override x.ReadJson(reader, t, _, serializer) =
    let innerType = t.GetGenericArguments().[0]

    let innerType =
      if innerType.IsValueType then
        typedefof<Nullable<_>>.MakeGenericType([| innerType |])
      else
        innerType

    let value = serializer.Deserialize(reader, innerType)
    let cases = FSharpType.GetUnionCases t

    if isNull value then
      FSharpValue.MakeUnion(cases.[0], [||])
    else
      FSharpValue.MakeUnion(cases.[1], [|value|])
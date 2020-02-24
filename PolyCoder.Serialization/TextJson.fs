namespace PolyCoder.Serialization

open System
open System.Text.Json

type SystemJsonSerializer(options: JsonSerializerOptions) =
    new() = SystemJsonSerializer(JsonSerializerOptions())

    interface ISerializer<obj, string> with
        member this.Serialize target =
            try
                JsonSerializer.Serialize<obj>(target, options) |> Ok
            with exn ->
                Error exn
        
        member this.Deserialize text =
            try
                JsonSerializer.Deserialize<obj>(text, options) |> Ok
            with exn ->
                Error exn
        
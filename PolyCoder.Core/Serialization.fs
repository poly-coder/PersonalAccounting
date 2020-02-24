namespace PolyCoder.Serialization

type ISerializer<'deserialized, 'serialized> =
    abstract Serialize: 'deserialized -> Result<'serialized, exn>
    abstract Deserialize: 'serialized -> Result<'deserialized, exn>

module Serializer =
    let from serialize deserialize =
        { new ISerializer<_, _> with
            member this.Serialize target = serialize target
            member this.Deserialize serialized = deserialize serialized
        }

    let asTupple (serializer: ISerializer<_, _>) =
        let serialize target = serializer.Serialize target
        let deserialize serialized = serializer.Deserialize serialized
        serialize, deserialize

    let bind inward outward (innerSerializer: ISerializer<_, _>) =
        { new ISerializer<_, _> with
            member this.Serialize target =
                target
                |> inward
                |> Result.bind innerSerializer.Serialize
        
            member this.Deserialize serialized =
                serialized
                |> innerSerializer.Deserialize
                |> Result.bind outward
        }

    let map inward outward = bind (inward >> Ok) (outward >> Ok)

    let pipe (outerSerializer: ISerializer<_, _>) =
        bind outerSerializer.Serialize outerSerializer.Deserialize

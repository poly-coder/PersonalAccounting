namespace PolyCoder

open System

type IClock =
    abstract UtcNow: unit -> DateTime

type SystemClock() =
    interface IClock with
        member _.UtcNow() = DateTime.UtcNow

type IIdGenerator =
    abstract NewId: unit -> string

type SystemGuidGenerator() =
    interface IIdGenerator with
        member _.NewId() = Guid.NewGuid().ToString("D")


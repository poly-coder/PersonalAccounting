namespace PolyCoder.EventSourcing

open System
open FSharp.Control

type StoreEvents = {
    entityId: string
    requestId: string
    currentVersion: int64
    timestamp: DateTime
    events: obj seq
    metadata: Map<string, string>
}

type StoreEventsResult = {
    envelopes: EventEnvelope array
}

type EntityEventsFilter = {
    minimumVersion: int64 option
    maximumVersion: int64 option
}

type ReadEvents = {
    entityId: string
    filter: EntityEventsFilter option
}

type ReadEventsSegment = {
    envelopes: EventEnvelope array
    isLastSegment: bool
}

type IEntityEventsStorage =
    abstract StoreEvents: StoreEvents -> Async<StoreEventsResult>
    abstract ReadEvents: ReadEvents -> AsyncSeq<ReadEventsSegment>

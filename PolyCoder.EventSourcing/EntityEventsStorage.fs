namespace PolyCoder.EventSourcing

open System
open FSharp.Control

type StoreEvents = {
    entityId: string
    partitionKey: string
    eventId: string
    requestId: string
    correlationId: string
    timestamp: DateTime
    version: int64
    event: obj
    metadata: Map<string, string>
}

type StoreEventsResult = {
    envelopes: EventEnvelope array
}

type ReadEventsFilter = {
    minimumVersion: int64 option
    maximumVersion: int64 option
    minimumTimestamp: DateTime option
    maximumTimestamp: DateTime option
    requestId: string option
    correlationId: string option
    eventId: string option
}

type ReadEvent = {
    partitionKey: string
    eventId: string
}

type ReadEventResult = {
    envelope: EventEnvelope option
}

type ReadEvents = {
    partitionKey: string
    filter: ReadEventsFilter option
}

type ReadEventsSegment = {
    envelopes: EventEnvelope array
    isLastSegment: bool option
}

type IEntityEventsStorage =
    abstract StoreEvents: StoreEvents -> Async<StoreEventsResult>
    abstract ReadEvent: ReadEvent -> Async<ReadEventResult>
    abstract ReadEvents: ReadEvents -> AsyncSeq<ReadEventsSegment>

type HandleEventContext = {
    envelope: EventEnvelope
    getRequest: unit -> Async<RequestEnvelope>
}

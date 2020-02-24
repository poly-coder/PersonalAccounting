namespace PolyCoder.EventSourcing

open System
open FSharp.Control



type StoreSnapshot = {
    entityId: string
    requestId: string
    version: int64
    timestamp: DateTime
    snapshot: obj
    snapshotVersion: string
    metadata: Map<string, string>
}

type ReadSnapshot = {
    entityId: string
    snapshotVersion: string
    filter: EntityEventsFilter option
}

type ReadSnapshotResult = {
    snapshot: obj
    entityId: string
    eventId: string
    timestamp: DateTime
    version: int64
    metadata: Map<string, string>
}

type IEntitySnapshotStorage =
    abstract StoreSnapshot: StoreSnapshot -> Async<unit>
    abstract ReadSnapshot: ReadSnapshot -> Async<ReadSnapshotResult option>



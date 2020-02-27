namespace PolyCoder.EventSourcing

open System
open FSharp.Control

type StoreSnapshot = {
    entityId: string
    partitionKey: string
    snapshotId: string
    timestamp: DateTime
    version: int64
    snapshot: obj
    metadata: Map<string, string>
}

type StoreSnapshotResult = {
    envelope: SnapshotEnvelope
}

type ReadSnapshot = {
    partitionKey: string
}

type ReadSnapshotResult = {
    snapshot: obj option
}

type IEntitySnapshotStorage =
    abstract StoreSnapshot: StoreSnapshot -> Async<StoreSnapshotResult>
    abstract ReadSnapshot: ReadSnapshot -> Async<ReadSnapshotResult>



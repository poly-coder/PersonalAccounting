namespace PolyCoder.EventSourcing

open System
open System.Security.Claims

type EventEnvelope = {
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

module EventEnvelope =
    let empty = Unchecked.defaultof<EventEnvelope>

    let withEntityId entityId envelope : EventEnvelope =
        { envelope with entityId = entityId }     

    let withPartitionKey partitionKey envelope : EventEnvelope =
        { envelope with partitionKey = partitionKey }     

    let withEventId eventId envelope : EventEnvelope =
        { envelope with eventId = eventId }     

    let withRequestId requestId envelope : EventEnvelope =
        { envelope with requestId = requestId }     

    let withCorrelationId correlationId envelope : EventEnvelope =
        { envelope with correlationId = correlationId }     

    let withTimestamp timestamp envelope : EventEnvelope =
        { envelope with timestamp = timestamp }     

    let withVersion version envelope : EventEnvelope =
        { envelope with version = version }     

    let withEvent event envelope : EventEnvelope =
        { envelope with event = event }     

    let withMetadata metadata envelope : EventEnvelope =
        { envelope with metadata = metadata }     

type RequestEnvelope = {
    entityId: string
    partitionKey: string
    requestId: string
    requestType: string
    correlationId: string
    timestamp: DateTime
    request: obj
    metadata: Map<string, string>
    principal: ClaimsPrincipal
}

module RequestEnvelope =
    let empty = Unchecked.defaultof<RequestEnvelope>

    let withEntityId entityId envelope : RequestEnvelope =
        { envelope with entityId = entityId }     

    let withPartitionKey partitionKey envelope : RequestEnvelope =
        { envelope with partitionKey = partitionKey }     

    let withRequestType requestType envelope : RequestEnvelope =
        { envelope with requestType = requestType }     

    let withRequestId requestId envelope : RequestEnvelope =
        { envelope with requestId = requestId }     

    let withCorrelationId correlationId envelope : RequestEnvelope =
        { envelope with correlationId = correlationId }     

    let withTimestamp timestamp envelope : RequestEnvelope =
        { envelope with timestamp = timestamp }     

    let withRequest request envelope : RequestEnvelope =
        { envelope with request = request }     

    let withMetadata metadata envelope : RequestEnvelope =
        { envelope with metadata = metadata }     

    let withPrincipal principal envelope : RequestEnvelope =
        { envelope with principal = principal }     

type SnapshotEnvelope = {
    entityId: string
    partitionKey: string
    snapshotId: string
    timestamp: DateTime
    version: int64
    snapshot: obj
    metadata: Map<string, string>
}


module SnapshotEnvelope =
    let empty = Unchecked.defaultof<SnapshotEnvelope>

    let withEntityId entityId envelope : SnapshotEnvelope =
        { envelope with entityId = entityId }     

    let withPartitionKey partitionKey envelope : SnapshotEnvelope =
        { envelope with partitionKey = partitionKey }     

    let withSnapshotId snapshotId envelope : SnapshotEnvelope =
        { envelope with snapshotId = snapshotId }     

    let withTimestamp timestamp envelope : SnapshotEnvelope =
        { envelope with timestamp = timestamp }     

    let withVersion version envelope : SnapshotEnvelope =
        { envelope with version = version }     

    let withSnapshot snapshot envelope : SnapshotEnvelope =
        { envelope with snapshot = snapshot }     

    let withMetadata metadata envelope : SnapshotEnvelope =
        { envelope with metadata = metadata }     

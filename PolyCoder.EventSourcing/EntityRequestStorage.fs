namespace PolyCoder.EventSourcing

open System
open FSharp.Control
open System.Security.Claims

type StoreRequest = {
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

module StoreRequest =
    let toEnvelope (request: StoreRequest) : RequestEnvelope = {
        entityId = request.entityId
        partitionKey = request.partitionKey
        requestId = request.requestId
        requestType = request.requestType
        correlationId = request.correlationId
        timestamp = request.timestamp
        request = request.request
        metadata = request.metadata
        principal = request.principal
    }

type StoreRequestResult = {
    envelope: RequestEnvelope
}

type ReadRequest = {
    requestId: string
    partitionKey: string
}

type ReadRequestResult = {
    envelope: RequestEnvelope option
}

type ReadRequestsFilter = {
    minimumTimestamp: DateTime option
    maximumTimestamp: DateTime option
    correlationId: string option
    requestType: string option
}

type ReadRequests = {
    partitionKey: string
    filter: ReadRequestsFilter option
}

type ReadRequestSegment = {
    envelopes: RequestEnvelope array
}

type IEntityRequestStorage =
    abstract StoreRequest: StoreRequest -> Async<StoreRequestResult>
    abstract ReadRequest: ReadRequest -> Async<ReadRequestResult>
    abstract ReadRequests: ReadRequests -> AsyncSeq<ReadRequestSegment>

type HandleRequestContext = {
    envelope: RequestEnvelope
}

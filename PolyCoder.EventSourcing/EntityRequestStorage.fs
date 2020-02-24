namespace PolyCoder.EventSourcing

open System
open FSharp.Control
open System.Security.Claims

type StoreRequest = {
    entityId: string
    requestId: string
    requestType: string
    timestamp: DateTime
    request: obj
    metadata: Map<string, string>
    principal: ClaimsPrincipal
}

type StoreRequestResult = {
    envelope: RequestEnvelope
}

type EntityRequestFilter = {
    requestId: string option
}

type ReadRequest = {
    entityId: string
    filter: EntityRequestFilter option
}

type ReadRequestSegment = {
    envelopes: RequestEnvelope array
    isLastSegment: bool
}

type IEntityRequestStorage =
    abstract StoreRequest: StoreRequest -> Async<StoreRequestResult>
    abstract ReadRequest: ReadRequest -> AsyncSeq<ReadRequestSegment>

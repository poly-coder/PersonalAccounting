namespace PolyCoder.EventSourcing

open System
open System.Security.Claims

type EventEnvelope = {
    entityId: string
    eventId: string
    requestId: string
    timestamp: DateTime
    version: int64
    event: obj
    metadata: Map<string, string>
}

type RequestEnvelope = {
    entityId: string
    requestId: string
    requestType: string
    timestamp: DateTime
    request: obj
    metadata: Map<string, string>
    principal: ClaimsPrincipal
}

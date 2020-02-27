namespace PolyCoder.EventSourcing

open System
open Newtonsoft.Json
open System.Collections.Generic
open PolyCoder
open PolyCoder.Validation
open System.Security.Claims

[<CLIMutable>]
type RequestEnvelopeDoc = {
    [<JsonProperty("id")>]
    requestId: string

    [<JsonProperty("partitionKey")>]
    partitionKey: string

    [<JsonProperty("entityId")>]
    entityId: string

    [<JsonProperty("requestType")>]
    requestType: string

    [<JsonProperty("correlationId")>]
    correlationId: string

    [<JsonProperty("timestamp")>]
    timestamp: DateTime

    [<JsonProperty("request", TypeNameHandling = TypeNameHandling.Objects)>]
    request: obj

    [<JsonProperty("metadata")>]
    metadata: Dictionary<string, string>

    [<JsonProperty("principal")>]
    principal: byte[]
}

module RequestEnvelopeDoc =
    open System.Collections.Generic
    open System.IO

    let serializeMetadata metadata =
        (Dictionary(), Map.toSeq metadata)
        ||> Seq.fold (fun dict (k, v) -> dict.Add(k, v); dict)

    let deserializeMetadata (metadata: #IDictionary<_, _>) =
        (Map.empty, metadata)
        ||> Seq.fold (fun map pair -> map |> Map.add pair.Key pair.Value)

    let serializePrincipal (principal: ClaimsPrincipal) =
        use stream = new MemoryStream()
        use writer = new BinaryWriter(stream)
        principal.WriteTo writer
        writer.Flush()
        stream.ToArray()

    let deserializePrincipal (bytes: byte[]) =
        use stream = new MemoryStream(bytes)
        use reader = new BinaryReader(stream)
        ClaimsPrincipal reader

    let createFrom (envelope: RequestEnvelope) : RequestEnvelopeDoc =
        {
            requestId     = envelope.requestId
            partitionKey  = envelope.partitionKey
            entityId      = envelope.entityId
            requestType   = envelope.requestType
            correlationId = envelope.correlationId
            timestamp     = envelope.timestamp
            request       = envelope.request
            metadata      = serializeMetadata envelope.metadata
            principal     = serializePrincipal envelope.principal
        }

    let toEnvelope (document: RequestEnvelopeDoc) : RequestEnvelope = {
        requestId     = document.requestId
        partitionKey  = document.partitionKey
        entityId      = document.entityId
        requestType   = document.requestType
        correlationId = document.correlationId
        timestamp     = document.timestamp
        request       = document.request
        metadata      = deserializeMetadata document.metadata
        principal     = deserializePrincipal document.principal
    }

module AzureCosmosEntityRequestStorageUtils =

    let validateStoreRequest (request: StoreRequest) =
        request
        |> validate id [
            validateProperty "entityId" (fun r -> r.entityId) [
                isNotNull
                hasMinLength 8
                hasMaxLength 100
            ]
            validateProperty "requestId" (fun r -> r.requestId) [
                isNotNull
                hasMinLength 8
                hasMaxLength 100
            ]
            validateProperty "requestType" (fun r -> r.requestType) [
                isNotNull
                hasMinLength 1
                hasMaxLength 20
            ]
        ]

    let toStoreRequestDoc (request: StoreRequest) =
        request
        |> validateStoreRequest
        |> Result.map (StoreRequest.toEnvelope >> RequestEnvelopeDoc.createFrom)

open AzureCosmosEntityRequestStorageUtils
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Linq
open System.Linq
open System.Net
open FSharp.Control

type AzureCosmosEntityRequestStorageRequirements = {
    requestsContainer: Async<Container>
}

type AzureCosmosEntityRequestStorage(reqs: AzureCosmosEntityRequestStorageRequirements) =

    let createQuery (partitionKey: string) (filter: ReadRequestsFilter option) (container: Container) =
        let q =
            query {
                for doc in container.GetItemLinqQueryable<RequestEnvelopeDoc>() do
                where (doc.partitionKey = partitionKey)
                sortBy doc.timestamp
                select doc
            }

        match filter with
        | None -> q
        | Some filter ->
            let q =
                match filter.correlationId with
                | Some cid -> query {
                    for doc in q do
                    where (doc.correlationId = cid)
                    select doc }
                | None -> q

            let q =
                match filter.requestType with
                | Some requestType -> query {
                    for doc in q do
                    where (doc.requestType = requestType)
                    select doc }
                | None -> q

            let q =
                match filter.minimumTimestamp with
                | Some ts -> query {
                    for doc in q do
                    where (doc.timestamp >= ts)
                    select doc }
                | None -> q

            let q =
                match filter.maximumTimestamp with
                | Some ts -> query {
                    for doc in q do
                    where (doc.timestamp <= ts)
                    select doc }
                | None -> q
            q

    interface IEntityRequestStorage with
        member this.StoreRequest input = async {
            let requestDoc =
                toStoreRequestDoc input
                |> Validate.unsafeRun // throws ValidationException if invalid

            let! container = reqs.requestsContainer
        
            let! response =
                container.CreateItemAsync(requestDoc)
                |> Async.AwaitTask

            response.StatusCode
                |> HttpStatus.throwIfNotSuccess "Error storing request"

            return {
                envelope = RequestEnvelopeDoc.toEnvelope response.Resource
            }
        }

        member this.ReadRequest input = async {
            let! container = reqs.requestsContainer

            try
                let partitionKey = PartitionKey input.partitionKey
                let! response =
                    container.ReadItemAsync(input.requestId, partitionKey)
                    |> Async.AwaitTask

                return {
                    envelope = RequestEnvelopeDoc.toEnvelope response.Resource |> Some
                }

            with
            | :? CosmosException as exn when exn.StatusCode = HttpStatusCode.NotFound ->
                return { envelope = None }
        }

        member this.ReadRequests input = asyncSeq {
            let! container = reqs.requestsContainer

            let query = container |> createQuery input.partitionKey input.filter
            
            let iterator = query.ToFeedIterator()

            while iterator.HasMoreResults do
                let! documents = iterator.ReadNextAsync() |> Async.AwaitTask

                documents.StatusCode |> HttpStatus.throwIfNotSuccess "Error reading requests"

                let envelopes =
                    documents
                    |> Seq.map RequestEnvelopeDoc.toEnvelope
                    |> Seq.toArray

                let segment : ReadRequestSegment = { envelopes = envelopes }

                yield segment
        }

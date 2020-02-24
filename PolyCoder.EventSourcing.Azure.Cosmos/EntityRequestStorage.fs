namespace PolyCoder.EventSourcing

open System
open Newtonsoft.Json
open System.Collections.Generic
open PolyCoder
open PolyCoder.Validation
open System.Security.Claims

[<CLIMutable>]
type ClaimDoc = {
    [<JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Include)>]
    type': string

    [<JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Include)>]
    value: string

    [<JsonProperty("valueType", DefaultValueHandling = DefaultValueHandling.Ignore)>]
    valueType: string

    [<JsonProperty("issuer", DefaultValueHandling = DefaultValueHandling.Ignore)>]
    issuer: string

    [<JsonProperty("originalIssuer", DefaultValueHandling = DefaultValueHandling.Ignore)>]
    originalIssuer: string
}

[<CLIMutable>]
type ClaimsIdentityDoc = {
    claims: ClaimDoc array
}

[<CLIMutable>]
type ClaimsPrincipalDoc = {
    identities: ClaimsIdentityDoc array
}

[<CLIMutable>]
type RequestEnvelopeDoc = {
    [<JsonProperty("entityId")>]
    entityId: string

    [<JsonProperty("requestId")>]
    requestId: string

    [<JsonProperty("requestType")>]
    requestType: string

    [<JsonProperty("timestamp")>]
    timestamp: DateTime

    [<JsonProperty("request", TypeNameHandling = TypeNameHandling.Objects)>]
    request: obj

    [<JsonProperty("metadata")>]
    metadata: Dictionary<string, string>

    [<JsonProperty("requestId")>]
    principal: ClaimsPrincipalDoc
}

module AzureCosmosEntityRequestStorageUtils =
    let toClaimDoc (claimm: Claim) : ClaimDoc =
        {
            type' = claimm.Type
            value = claimm.Value
            valueType = claimm.ValueType
            issuer = claimm.Issuer
            originalIssuer = claimm.OriginalIssuer
        }

    let toIdentityDoc (identity: ClaimsIdentity) : ClaimsIdentityDoc =
        {
            claims = identity.Claims |> Seq.map toClaimDoc |> Seq.toArray
        }

    let toPrincipalDoc (principal: ClaimsPrincipal) : ClaimsPrincipalDoc =
        {
            identities = principal.Identities |> Seq.map toIdentityDoc |> Seq.toArray
        }

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
                hasMaxLength 100
            ]
        ]

    let toStoreRequestDoc (request: StoreRequest) =
        request
        |> validateStoreRequest
        |> Result.map (fun req ->
            let doc: RequestEnvelopeDoc = {
                entityId = req.entityId
                requestId = req.requestId
                requestType = req.requestType
                timestamp = req.timestamp
                request = req.request
                metadata = req.metadata |> Map.toDict
                principal = req.principal |> toPrincipalDoc
            }
            doc
        )

open AzureCosmosEntityRequestStorageUtils
open Microsoft.Azure.Cosmos
open System.Net

type AzureCosmosEntityRequestStorageRequirements = {
    requestsContainer: Async<Container>
}

type AzureCosmosEntityRequestStorage(reqs: AzureCosmosEntityRequestStorageRequirements) =

    member this.StoreRequest (request: StoreRequest) : Async<StoreRequestResult> = async {
        let requestDoc = toStoreRequestDoc request |> Validate.unsafeRun // throws ValidationException if invalid

        let! container = reqs.requestsContainer
        
        let! itemResponse = container.CreateItemAsync(requestDoc, Nullable <| PartitionKey(request.entityId)) |> Async.AwaitTask

        if itemResponse.StatusCode >= HttpStatusCode.BadRequest then
            // TODO: Create exceptions to represent Http status
            invalidOp (sprintf "Error creating cosmos document: %A" itemResponse.StatusCode)
        
        return {
            envelope = {
                entityId = request.entityId
                requestId = request.requestId
                requestType = request.requestType
                timestamp = request.timestamp
                request = request.request
                metadata = request.metadata
                principal = request.principal
            }
        }
    }

    member this.ReadRequest (request: ReadRequest) =
        raise (NotImplementedException())

    interface IEntityRequestStorage with
        member this.StoreRequest request = this.StoreRequest request
        member this.ReadRequest request = this.ReadRequest request
    
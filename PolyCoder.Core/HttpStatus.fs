namespace PolyCoder

open System.Net

exception HttpContentException of HttpStatusCode * string

type ResultContent<'t> = {
    success: bool
    message: string
    data: 't
    errors: Map<string, string list>
}

type ResultContent = ResultContent<obj>

module ResultContent =
    let successful data message : ResultContent<_> = {
        success = true
        message = message
        data = data
        errors = Map.empty
    }

    let failure message errors : ResultContent =
        let errors =
            errors
            |> Seq.groupBy fst
            |> Seq.map (fun (name, pairs) -> name, pairs |> Seq.map snd |> Seq.toList)
            |> Map.ofSeq
        {
            success = false
            message = message
            data = null
            errors = errors
        }

module HttpStatus =

    let throwIfNotSuccess message (code: HttpStatusCode) =
        if code >= HttpStatusCode.BadRequest then
            raise <| HttpContentException(code, message)


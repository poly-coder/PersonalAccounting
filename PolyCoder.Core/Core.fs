namespace PolyCoder

[<AutoOpen>]
module Preamble =
    let konst x _ = x

module Seq =
    open System
    open System.Linq
    
    let toDictionaryK keyFn (source: _ seq) =
        source.ToDictionary(Func<_, _> keyFn)

    let toDictionaryKV keyFn valueFn (source: _ seq) =
        source.ToDictionary(Func<_, _> keyFn, Func<_, _> valueFn)

module Option =
    let matchWith fSome fNone = function
    | Some x -> fSome x
    | None -> fNone()

module Map =
    let toDict map =
        Map.toSeq map
        |> Seq.toDictionaryKV fst snd

module Async =
    open System.Threading.Tasks

    let memoize (computation: Async<_>) =
        let mutable taskSource: TaskCompletionSource<_> = Unchecked.defaultof<_>
        let lockObj = obj()
        async {
            if isNull taskSource then
                lock lockObj (fun () ->
                    if isNull taskSource then
                        taskSource <- TaskCompletionSource()
                        async {
                            try
                                let! result = computation
                                taskSource.SetResult(result)
                            with
                            | :? TaskCanceledException ->
                                taskSource.SetCanceled()
                            | exn ->
                                taskSource.SetException(exn)
                        } |> Async.Start
                )
            return! taskSource.Task |> Async.AwaitTask
        }
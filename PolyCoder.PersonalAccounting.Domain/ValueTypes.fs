namespace PolyCoder.PersonalAccounting.Domain

open PolyCoder.Validation

type AccountId = AccountId of string

type AccountName = AccountName of string
            

[<AutoOpen>]
module Validations =
    let asAccountId id =
        Validate.zero
        |> isNotNull id
    //    Validation.zero
    //        |> Validation.and' 
    //validation {
    //    do! isNotNull id
    //    do! hasMinLength 4 id
    //}
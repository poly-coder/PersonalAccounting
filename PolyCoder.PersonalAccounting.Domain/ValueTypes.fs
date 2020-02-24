namespace PolyCoder.PersonalAccounting.Domain

open PolyCoder.Validation

type AccountId = AccountId of string

type AccountName = AccountName of string

[<AutoOpen>]
module Validations =
    let accountId = validate AccountId [ isNotNull; isNotBlank; hasMinLength 4; hasMaxLength 100 ]
    let accountName = validate AccountName [ isNotNull; isNotBlank; hasMinLength 4; hasMaxLength 100 ]

namespace PolyCoder.PersonalAccounting.Domain

[<RequireQualifiedAccess>]
module AccountInstance =
    type Command =
        | CreateAccount of AccountName
        | RenameAccount of AccountName
        | DisableAccount
        | EnableAccount

    type Event =
        | AccountWasCreated of AccountId * AccountName
        | AccountWasRenamed of AccountId * AccountName
        | AccountWasDisabled of AccountId
        | AccountWasEnabled of AccountId
        | AccountWasSetAsDefault of AccountId

[<RequireQualifiedAccess>]
module AccountCollection =
    type Command =
        | CreateAccount of AccountId * AccountName
        | RenameAccount of AccountId * AccountName
        | DisableAccount of AccountId
        | EnableAccount of AccountId
        | SetAsDefaultAccount of AccountId

    type Event =
        | AccountWasCreated of AccountId * AccountName
        | AccountWasRenamed of AccountId * AccountName
        | AccountWasDisabled of AccountId
        | AccountWasEnabled of AccountId
        | AccountWasSetAsDefault of AccountId

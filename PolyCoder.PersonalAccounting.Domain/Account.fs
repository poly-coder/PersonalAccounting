namespace PolyCoder.PersonalAccounting.Domain

open PolyCoder.Validation

[<RequireQualifiedAccess>]
module AccountInstance =
    type Command =
        | CreateAccount of AccountName
        | RenameAccount of AccountName
        | DisableAccount
        | EnableAccount

    type Event =
        | AccountWasCreated of AccountName
        | AccountWasRenamed of AccountName
        | AccountWasDisabled
        | AccountWasEnabled

    type State =
        | Inexistent
        | AccountState of AccountState

    and AccountState = {
        name: AccountName
        isEnabled: bool
    }

    let initState() = Inexistent

    let applyEvent state event =
        match state, event with
        | Inexistent, AccountWasCreated name ->
            AccountState {
                name = name
                isEnabled = false }

        | AccountState state, AccountWasRenamed name ->
            AccountState {
                state with
                    name = name }

        | AccountState state, AccountWasDisabled ->
            AccountState {
                state with
                    isEnabled = false }

        | AccountState state, AccountWasEnabled ->
            AccountState {
                state with
                    isEnabled = true }


        | Inexistent, _ -> state

        | AccountState _, _ -> state


    let handleCommand state command =
        match state, command with
        | Inexistent, CreateAccount name ->
            Validate.valid [ 
                AccountWasCreated name
                AccountWasEnabled
            ]

        | Inexistent, _ ->
            Validate.invalid' (messageError "The account is not created yet")

        | AccountState _, CreateAccount _ ->
            Validate.invalid' (messageError "The account is already created")

        | AccountState state, RenameAccount newName ->
            if state.name <> newName then
                Validate.valid [ AccountWasRenamed newName ]
            else
                Validate.valid []

        | AccountState state, DisableAccount ->
            if state.isEnabled then
                Validate.valid [ AccountWasDisabled ]
            else
                Validate.valid []

        | AccountState state, EnableAccount ->
            if not state.isEnabled then
                Validate.valid [ AccountWasEnabled ]
            else
                Validate.valid []

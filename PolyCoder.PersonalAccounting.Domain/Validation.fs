namespace PolyCoder.Validation

type ValidationError =
    | ValidationError of msg: string
    | ValidationException of exn: exn
    | PropertyError of propertyName: string * ValidationError list
    | IndexedError of index: int * ValidationError list

[<AutoOpen>]
module ValidationError =
    let messageError msg = ValidationError msg
    let exnError exn = ValidationException exn
    let propertyError prop errors = PropertyError (prop, List.rev errors)
    let indexError index errors = IndexedError (index, List.rev errors)

type ValidationResult<'a, 'e> = Result<'a, 'e list>

type Validator<'a, 'b, 'e> = 'a -> ValidationResult<'b, 'e>

exception ValidationException of ValidationError list

module Validate =

    let valid a : ValidationResult<_, _> = Result.Ok a
    
    let invalid (errors: _ list) : ValidationResult<_, _> = Result.Error errors
    
    let invalid' error = invalid [ error ]

    let zero<'e> : ValidationResult<unit, 'e> = valid ()

    let matchWith fValid fInvalid (ma: ValidationResult<_, _>) =
        match ma with
        | Result.Ok result -> fValid result
        | Result.Error errors -> fInvalid errors

    let bind f = matchWith f invalid

    let map f = bind (f >> valid)

    let bindErrors f = matchWith valid f
    
    let mapErrors f = bindErrors (f >> invalid)
    
    let mapError f = mapErrors (List.map f)

    let withError error = mapErrors (fun errors -> error :: errors)
    
    let withResult result = map (fun _ -> result)

    let unsafeRun (ma: ValidationResult<_, _>) = ma |> matchWith id (List.rev >> ValidationException >> raise)

module ValidationPredicates =
    open Validate
    open System.Text.RegularExpressions

    let withPredicate predicateFn messageFn value =
        bind (fun result ->
            if predicateFn value then result |> valid
            else messageFn() |> messageError |> invalid')

    let withPredicate1 arg1 predicateFn messageFn value =
        bind (fun result ->
            if predicateFn arg1 value then result |> valid
            else messageFn arg1 |> messageError |> invalid')

    let withPredicate2 arg1 arg2 predicateFn messageFn value =
        bind (fun result ->
            if predicateFn arg1 arg2 value then result |> valid
            else messageFn arg1 arg2 |> messageError |> invalid')

    let (>&&>) fn1 fn2 = fun value -> fn1 value && fn2 value
    let (>||>) fn1 fn2 = fun value -> fn1 value || fn2 value

    // Generic
    let isNotNullPredicate value = obj.ReferenceEquals(value, null) |> not
    
    // String
    let isNotEmptyPredicate value = System.String.IsNullOrEmpty(value) |> not
    let isNotBlankPredicate value = System.String.IsNullOrWhiteSpace(value) |> not
    let hasLengthPredicate length value = String.length value = length
    let hasMinLengthPredicate minLength value = String.length value >= minLength
    let hasMaxLengthPredicate maxLength value = String.length value <= maxLength
    let hasLengthBetweenPredicate minLength maxLength = hasMinLengthPredicate minLength >&&> hasMaxLengthPredicate maxLength
    let matchesRegexPredicate (regex: Regex) value = regex.IsMatch(value)
    let matchesPredicate pattern = Regex(pattern) |> matchesRegexPredicate

module ValidationResources =
    open System
    open System.Resources
    open System.Reflection

    let konst x _ = x

    let manager = lazy ResourceManager("ValidationResources", Assembly.GetExecutingAssembly())
    let getUICulture() = Threading.Thread.CurrentThread.CurrentUICulture
    let getCulture() = Threading.Thread.CurrentThread.CurrentCulture
    let getString name info = manager.Value.GetString(name, info)
    let getString1 name (param1: 'a) info = String.Format(manager.Value.GetString(name, info), param1)
    let getString2 name (param1: 'a) (param2: 'b) info = String.Format(manager.Value.GetString(name, info), param1, param2)

    let CannotBeNullAs = getString "CannotBeNull"
    let CannotBeNull () = getUICulture() |> CannotBeNullAs

    let CannotBeEmptyAs = getString "CannotBeEmpty"
    let CannotBeEmpty () = getUICulture() |> CannotBeEmptyAs

    let CannotBeBlankAs  = getString "CannotBeBlank"
    let CannotBeBlank () = getUICulture() |> CannotBeBlankAs

    let MustHaveLengthAs (length: int)  = getString1 "MustHaveLength" length
    let MustHaveLength length = getUICulture() |> MustHaveLengthAs length

    let MustHaveMinLengthAs (minLength: int)  = getString1 "MustHaveMinLength" minLength
    let MustHaveMinLength minLength = getUICulture() |> MustHaveMinLengthAs minLength

    let MustHaveMaxLengthAs (maxLength: int)  = getString1 "MustHaveMaxLength" maxLength
    let MustHaveMaxLength maxLength = getUICulture() |> MustHaveMaxLengthAs maxLength

    let MustHaveLengthBetweenAs (minLength: int) (maxLength: int)  = getString2 "MustHaveLengthBetween" minLength maxLength
    let MustHaveLengthBetween minLength maxLength = getUICulture() |> MustHaveLengthBetweenAs minLength maxLength

[<AutoOpen>]
module Validations =
    open ValidationPredicates
    open ValidationResources

    // Generic
    let isNotNullIntl messageFn value = value |> withPredicate isNotNullPredicate messageFn
    let isNotNull value = value |> isNotNullIntl CannotBeNull

    // String
    let isNotEmptyIntl messageFn value = value |> withPredicate isNotEmptyPredicate messageFn
    let isNotEmpty value = value |> isNotEmptyIntl CannotBeEmpty

    let isNotBlankIntl messageFn value = value |> withPredicate isNotBlankPredicate messageFn
    let isNotBlank value = value |> isNotBlankIntl CannotBeBlank
    
    let hasLengthIntl length messageFn = withPredicate1 length hasLengthPredicate messageFn
    let hasLength length = hasLengthIntl length MustHaveLength

    let hasMinLengthIntl minLength messageFn = withPredicate1 minLength hasMinLengthPredicate messageFn
    let hasMinLength minLength = hasMinLengthIntl minLength MustHaveMinLength

    let hasMaxLengthIntl maxLength messageFn = withPredicate1 maxLength hasMaxLengthPredicate messageFn
    let hasMaxLength maxLength = hasMaxLengthIntl maxLength MustHaveMaxLength

    let hasLengthBetweenIntl minLength maxLength messageFn = withPredicate2 minLength maxLength hasLengthBetweenPredicate messageFn
    let hasLengthBetween minLength maxLength = hasLengthBetweenIntl minLength maxLength MustHaveLengthBetween
    
    let matchesRegexIntl regex messageFn = withPredicate1 regex matchesRegexPredicate (ignore >> messageFn)
    let matchesRegex regex message = matchesRegexIntl regex (konst message)

    let matchesIntl pattern messageFn = withPredicate1 pattern matchesPredicate (ignore >> messageFn)
    let matches pattern message = matchesIntl pattern (konst message)

    // Comparables, =, !=, <, <=, >, >=, <= && <= and NOTs
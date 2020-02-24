namespace PolyCoder.Validation

type ValidationResult<'a, 'e> = Result<'a, 'e list>

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

type ValidationResult<'a> = ValidationResult<'a, ValidationError>

type Validator<'a, 'b, 'e> = 'a -> ValidationResult<'b, 'e>
type Validator<'a, 'b> = 'a -> ValidationResult<'b>

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

    let merge ma mb : ValidationResult<_, _> =
        match ma, mb with
        | Ok _, Ok b -> Ok b
        | Ok _, Error errors -> Error errors
        | Error errors, Ok _ -> Error errors
        | Error errorsOnA, Error errorsOnB -> Error (errorsOnA @ errorsOnB)

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

    let oppositePredicate predicateFn = predicateFn >> not
    let oppositePredicate1 arg1 predicateFn = predicateFn arg1 >> not
    let oppositePredicate2 arg1 arg2 predicateFn = predicateFn arg1 arg2 >> not

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

    // Comparables
    let isEqualsToPredicate other value = other = value
    let isDistinctToPredicate other value = other <> value
    
    let isLessThanPredicate other value = value < other
    let isLessThanOrEqualsToPredicate other value = value <= other
    let isGreaterThanPredicate other value = value > other
    let isGreaterThanOrEqualsToPredicate other value = value >= other
    let isBetweenPredicate minValue maxValue = isGreaterThanOrEqualsToPredicate minValue >&&> isLessThanOrEqualsToPredicate maxValue
    
    let isNotLessThanPredicate other = oppositePredicate1 isLessThanPredicate other
    let isNotLessThanOrEqualsToPredicate other = oppositePredicate1 isLessThanOrEqualsToPredicate other
    let isNotGreaterThanPredicate other = oppositePredicate1 isGreaterThanPredicate other
    let isNotGreaterThanOrEqualsToPredicate other = oppositePredicate1 isGreaterThanOrEqualsToPredicate other

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

    let MustBeEqualsToAs (maxLength: int)  = getString1 "MustBeEqualsTo" maxLength
    let MustBeEqualsTo maxLength = getUICulture() |> MustBeEqualsToAs maxLength

    let MustBeDistinctToAs (maxLength: int)  = getString1 "MustBeDistinctTo" maxLength
    let MustBeDistinctTo maxLength = getUICulture() |> MustBeDistinctToAs maxLength

    let MustBeGreaterThanAs (maxLength: int)  = getString1 "MustBeGreaterThan" maxLength
    let MustBeGreaterThan maxLength = getUICulture() |> MustBeGreaterThanAs maxLength

    let MustBeGreaterThanOrEqualsToAs (maxLength: int)  = getString1 "MustBeGreaterThanOrEqualsTo" maxLength
    let MustBeGreaterThanOrEqualsTo maxLength = getUICulture() |> MustBeGreaterThanOrEqualsToAs maxLength

    let MustBeLessThanAs (maxLength: int)  = getString1 "MustBeLessThan" maxLength
    let MustBeLessThan maxLength = getUICulture() |> MustBeLessThanAs maxLength

    let MustBeLessThanOrEqualsToAs (maxLength: int)  = getString1 "MustBeLessThanOrEqualsTo" maxLength
    let MustBeLessThanOrEqualsTo maxLength = getUICulture() |> MustBeLessThanOrEqualsToAs maxLength

    let MustBeBetweenAs (minValue: int) (maxValue: int)  = getString2 "MustBeBetween" minValue maxValue
    let MustBeBetween minValue maxValue = getUICulture() |> MustBeBetweenAs minValue maxValue

    let MustNotBeGreaterThanAs (maxLength: int)  = getString1 "MustNotBeGreaterThan" maxLength
    let MustNotBeGreaterThan maxLength = getUICulture() |> MustNotBeGreaterThanAs maxLength

    let MustNotBeGreaterThanOrEqualsToAs (maxLength: int)  = getString1 "MustNotBeGreaterThanOrEqualsTo" maxLength
    let MustNotBeGreaterThanOrEqualsTo maxLength = getUICulture() |> MustNotBeGreaterThanOrEqualsToAs maxLength

    let MustNotBeLessThanAs (maxLength: int)  = getString1 "MustNotBeLessThan" maxLength
    let MustNotBeLessThan maxLength = getUICulture() |> MustNotBeLessThanAs maxLength

    let MustNotBeLessThanOrEqualsToAs (maxLength: int)  = getString1 "MustNotBeLessThanOrEqualsTo" maxLength
    let MustNotBeLessThanOrEqualsTo maxLength = getUICulture() |> MustNotBeLessThanOrEqualsToAs maxLength

    let MustNotBeBetweenAs (maxLength: int)  = getString1 "MustNotBeBetween" maxLength
    let MustNotBeBetween maxLength = getUICulture() |> MustNotBeBetweenAs maxLength

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

    let isEqualsToIntl other messageFn = withPredicate1 other isEqualsToPredicate messageFn
    let isEqualsTo other = isEqualsToIntl other MustBeEqualsTo
    let isEQ other = isEqualsTo other

    let isDistinctToIntl other messageFn = withPredicate1 other isDistinctToPredicate messageFn
    let isDistinctTo other = isDistinctToIntl other MustBeDistinctTo
    let isNEQ other = isDistinctTo other

    let isLessThanIntl other messageFn = withPredicate1 other isLessThanPredicate messageFn
    let isLessThan other = isLessThanIntl other MustBeLessThan
    let isLT other = isLessThan other

    let isLessThanOrEqualsToIntl other messageFn = withPredicate1 other isLessThanOrEqualsToPredicate messageFn
    let isLessThanOrEqualsTo other = isLessThanOrEqualsToIntl other MustBeLessThanOrEqualsTo
    let isLE other = isLessThanOrEqualsTo other

    let isGreaterThanIntl other messageFn = withPredicate1 other isGreaterThanPredicate messageFn
    let isGreaterThan other = isGreaterThanIntl other MustBeGreaterThan
    let isGT other = isGreaterThan other

    let isGreaterThanOrEqualsToIntl other messageFn = withPredicate1 other isGreaterThanOrEqualsToPredicate messageFn
    let isGreaterThanOrEqualsTo other = isGreaterThanOrEqualsToIntl other MustBeGreaterThanOrEqualsTo
    let isGE other = isGreaterThanOrEqualsTo other

    let isBetweenIntl minValue maxValue messageFn = withPredicate2 minValue maxValue isBetweenPredicate messageFn
    let isBetween minValue maxValue = isBetweenIntl minValue maxValue MustBeBetween

    let validate mapper validations : Validator<_, _, _> =
        fun value ->
            let validationResult = 
                (Validate.zero, validations)
                ||> Seq.fold (fun prev validation ->
                    prev |> validation value)
            validationResult |> Validate.map (konst value >> mapper)

    let validateProperty propertyName getValue validations =
        fun value previousResult ->
            let propertyValue = getValue value
            let propertyValidationResult =
                validate ignore validations propertyValue
                |> Validate.mapErrors (propertyError propertyName >> List.singleton)
            propertyValidationResult |> Validate.merge previousResult

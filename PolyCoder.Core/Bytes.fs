module PolyCoder.Bytes

open System.Text
open System
open System.Globalization
open System.Security.Cryptography

let stringToUtf8 (str: string) = Encoding.UTF8.GetBytes(str)
let utf8ToString (bytes: byte[]) = Encoding.UTF8.GetString(bytes)

let fromBase64 (str: string) = Convert.FromBase64String(str)
let toBase64 (bytes: byte[]) = Convert.ToBase64String(bytes)

// TODO: Optimize this functions to work directly with chars instead of substrings
let toHexString (bytes: byte[]) =
    if bytes.Length = 0 then "" else
    let builder = StringBuilder(bytes.Length * 2)
    for i = 0 to bytes.Length - 1 do
        let str = bytes.[i].ToString("X2")
        builder.Append(str) |> ignore
    builder.ToString()

let toHexStringLower (bytes: byte[]) =
    if bytes.Length = 0 then "" else
    let builder = StringBuilder(bytes.Length * 2)
    for i = 0 to bytes.Length - 1 do
        let str = bytes.[i].ToString("x2")
        builder.Append(str) |> ignore
    builder.ToString()

let fromHexString (str: string) =
    if str.Length = 0 then
        Array.empty
    elif str.Length % 2 <> 0 then
        invalidOp "Hex string must have even amount of characters"
    else
        let bytes = Array.zeroCreate (str.Length / 2)
        for i = 0 to bytes.Length - 1 do
            bytes.[i] <- Byte.Parse(str.Substring(i * 2, 2), NumberStyles.HexNumber)
        bytes

let genRandomOn (bytes: byte[]) =
    use rng = new RNGCryptoServiceProvider()
    rng.GetBytes(bytes)

let genRandom size =
    let bytes = Array.zeroCreate size
    genRandomOn bytes
    bytes

let genNonZeroRandomOn (bytes: byte[]) =
    use rng = new RNGCryptoServiceProvider()
    rng.GetNonZeroBytes(bytes)

let genNonZeroRandom size =
    let bytes = Array.zeroCreate size
    genNonZeroRandomOn bytes
    bytes

let getSHA1Bytes (bytes: byte[]) =
    use hasher = new SHA1Managed()
    hasher.ComputeHash(bytes)

let getShortSHA1Bytes (bytes: byte[]) =
    let length = 5
    let hash = getSHA1Bytes bytes
    let response = Array.zeroCreate length
    Array.blit hash 0 response 0 length
    response

let stringToSha1 = stringToUtf8 >> getSHA1Bytes >> toHexStringLower
let stringToShortSha1 = stringToUtf8 >> getShortSHA1Bytes >> toHexStringLower

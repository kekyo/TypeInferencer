////////////////////////////////////////////////////////////////////////////
//
// Type inference implementation both Algorithm W and Algorithm M in F#
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace TypeInferencer

///////////////////////////////////////////////////////////////////////

[<Sealed>]
type internal InferState() =
    let mutable tiSupply = 0
    member _.newTyVar (prefix:string) =
        let index = tiSupply
        tiSupply <- tiSupply + 1
        TVar $"{prefix}{index}"

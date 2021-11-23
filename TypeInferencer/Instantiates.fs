////////////////////////////////////////////////////////////////////////////
//
// Type inference implementation both Algorithm W and Algorithm M in F#
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace TypeInferencer

open type FreeTypeVariables

///////////////////////////////////////////////////////////////////////

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<AutoOpen>]
module internal Instantiates =

    let newTyVar prefix (state:InferState) =
        state.newTyVar prefix
    let instantiate (Scheme(vars, typ):Scheme) state =
        let nvars = Seq.map (fun _ -> newTyVar "a" state) vars
        let s = Map.ofSeq (Seq.zip vars nvars)
        apply typ s

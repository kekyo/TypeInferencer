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

type public InferAlgorithm =
    | TopDown
    | BottomUp

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<AutoOpen>]
module public Inferencer =

    let infer algorithm env expr =
        match algorithm with
        | TopDown ->
            TypeInferencer.AlgorithmW.infer env expr
        | BottomUp ->
            TypeInferencer.AlgorithmM.infer env expr

///////////////////////////////////////////////////////////////////////

type public Algorithm =
    | TopDown = 0
    | BottomUp = 1

[<Sealed; AbstractClass>]
type public Inferencer =

    static member infer algorithm env expr =
        match algorithm with
        | Algorithm.TopDown ->
            TypeInferencer.AlgorithmW.infer env expr
        | Algorithm.BottomUp ->
            TypeInferencer.AlgorithmM.infer env expr
        | _ ->
            raise (new System.ArgumentException())

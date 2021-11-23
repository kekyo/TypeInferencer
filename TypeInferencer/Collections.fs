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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Map =

    let inline singleton (key:'Key when 'Key: comparison) (value:'Value) =
        Map [key, value]
    let inline map mapper m =
        Map.map (fun _ -> mapper) m
    let inline union (m1:Map<'Key, 'Value> when 'Key: comparison) (m2:Map<'Key, 'Value>) =
        Seq.append m1 m2
        |> Seq.distinctBy (fun kv -> kv.Key)
        |> Seq.map (fun kv -> kv.Key, kv.Value)
        |> Map.ofSeq
    let inline elems (m:Map<'Key, 'Value> when 'Key: comparison) =
        Set m.Values

///////////////////////////////////////////////////////////////////////

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Set =

    let inline singleton value =
        Set [value]
    let inline union (s1:Set<'T>) (s2:Set<'T>) =
        Seq.append s1 s2
        |> Seq.distinct
        |> Set.ofSeq

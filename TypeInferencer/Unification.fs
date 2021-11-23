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
module internal Unification =
    
    let private varBind name typ =
        match typ with
        | TVar n when n = name -> nullSubst
        | _ ->
            if Set.contains name (ftv typ) then
                raise (new OccurrenceException(name, typ))
            else
                Map.singleton name typ

    let rec mgu type1 type2 state =
        match type1, type2 with
        | TFun(l, r), TFun(l', r') ->
            let s1 = mgu l l' state
            let s2 = mgu (apply r s1) (apply r' s1) state
            composeSubst s1 s2
        | TVar name, typ ->
            varBind name typ
        | typ, TVar name ->
            varBind name typ
        | TInt, TInt ->
            nullSubst
        | TBool, TBool ->
            nullSubst
        | _ ->
            raise (new UnificationException(type1, type2))

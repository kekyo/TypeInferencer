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
module private FreeTypeVariables =

    let rec ftvType typ =
        match typ with
        | TVar name ->
            Set.singleton name
        | TInt ->
            Set.empty
        | TBool ->
            Set.empty
        | TFun (parameterType, resultType) ->
            Set.union (ftvType parameterType) (ftvType resultType)
    let rec applyType (s:Subst) typ =
        match typ with
        | TVar name ->
            match Map.tryFind name s with
            | Some foundType when foundType <> typ ->
                applyType s foundType
            | _ ->
                typ
        | TFun (parameterType, resultType) ->
            TFun (applyType s parameterType, applyType s resultType)
        | _ ->
            typ

    let ftvScheme (Scheme(vars, typ):Scheme) =
        Set.difference (ftvType typ) vars
    let applyScheme (s:Subst) (Scheme(vars, typ):Scheme) =
        Scheme(vars, (applyType (Seq.foldBack Map.remove vars s) typ))

    let nullSubst : Subst =
        Map.empty
    let composeSubst (s1:Subst) (s2:Subst) : Subst =
        Map.union (Map.map (applyType s1) s2) s1

    let inline private ftvs f xs =
        Seq.foldBack Set.union (Set.map f xs) Set.empty
    let inline private applies f s xs =
        Seq.map (f s) xs

    let ftvTypes types =
        ftvs ftvType types
    let applyTypes s types =
        applies applyType s types
        
    let ftvSchemes schemes =
        ftvs ftvScheme schemes
    let applySchemes s schemes =
        applies applyScheme s schemes

    let ftvTypeEnv (env:TypeEnv) =
        ftvSchemes (Map.elems env)
    let applyTypeEnv (s:Subst) (env:TypeEnv) : TypeEnv =
        Map.map (applyScheme s) env

    let generalize env typ =
        let vars = Set.difference (ftvType typ) (ftvTypeEnv env)
        Scheme(vars, typ)

///////////////////////////////////////////////////////////////////////

[<Sealed; AbstractClass>]
type internal FreeTypeVariables =

    static member inline ftv typ =
        FreeTypeVariables.ftvType typ
    static member inline ftv scheme =
        FreeTypeVariables.ftvScheme scheme
    static member inline ftv types =
        FreeTypeVariables.ftvTypes types
    static member inline ftv schemes =
        FreeTypeVariables.ftvSchemes schemes
    static member inline ftv env =
        FreeTypeVariables.ftvTypeEnv env

    static member inline apply typ =
        fun s -> FreeTypeVariables.applyType s typ
    static member inline apply scheme =
        fun s -> FreeTypeVariables.applyScheme s scheme
    static member inline apply types =
        fun s -> FreeTypeVariables.applyTypes s types
    static member inline apply schemes =
        fun s -> FreeTypeVariables.applySchemes s schemes
    static member inline apply env =
        fun s -> FreeTypeVariables.applyTypeEnv s env

    static member inline nullSubst =
        FreeTypeVariables.nullSubst
    static member inline composeSubst s1 s2 =
        FreeTypeVariables.composeSubst s1 s2

    static member inline generalize env typ =
        FreeTypeVariables.generalize env typ

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
module private AlgorithmW =

    let rec infer (env:TypeEnv) expr (state:InferState) =
        match expr with
        
        // W(Γ,x) = (id,{β/α}τ)
        //          where Γ(x) = ∀α.τ, new β
        | EVar name ->
            match Map.tryFind name env with
            | Some sigma ->
                let typ = instantiate sigma state
                nullSubst, typ
            | _ ->
                raise (new UnboundVariableException(name))
                
        // W(Γ,()) = (id,ι)
        | ELit literal ->
            match literal with
            | LInt _ ->
                nullSubst, TInt
            | LBool _ ->
                nullSubst, TBool
                
        // W(Γ,λx.e) =
        //   let (S1,τ1) = W(Γ+x:β,e), new β
        //   in (S1,S1 β → τ1)
        | EAbs(name, expr) ->
            let tv = newTyVar "a" state
            let env' = Map.add name (Scheme.ofType tv) env
            let s1, t1 = infer env' expr state
            s1, TFun(apply tv s1, t1)
            
        // W(Γ,e1 e2) =
        //   let (S1,τ1) = W(Γ,e1)
        //       (S2,τ2) = W(S1 Γ,e2)
        //       S3 = U(S2 τ1,τ2 → β), new β
        //   in (S3 S2 S1,S3 β)
        | EApp(func, arg) ->
            let tv = newTyVar "a" state
            let s1, t1 = infer env func state
            let s2, t2 = infer (apply env s1) arg state
            let s3 = mgu (apply t1 s2) (TFun(t2, tv)) state
            composeSubst (composeSubst s3 s2) s1, apply tv s3
            
        // W(Γ,let x = e1 in e2) =
        //   let (S1,τ1) = W(Γ,e1)
        //       (S2,τ2) = W(S1 Γ+x:Clos(S1 Γ,τ1),e2)
        //   in (S2 S1,τ2)
        | ELet(name, expr, body) ->
            let s1, t1 = infer env expr state
            let s' = generalize (apply env s1) t1
            let env' = Map.add name s' env
            let s2, t2 = infer (apply env' s1) body state
            composeSubst s2 s1, t2
            
        // W(Γ,fix f λx.e) =
        //   let (S1,τ1) = W(Γ+f:β,λx.e), new β
        //       S2 = U(S1 β,τ1)
        //   in (S2 S1,S2 τ1)
        | EFix(func, name, body) ->
            let tv = newTyVar "a" state
            let env'' = Map.add func (Scheme.ofType tv) env
            let s1, t1 = infer env'' (EAbs(name, body)) state
            let s2 = mgu (apply tv s1) t1 state
            composeSubst s2 s1, apply t1 s2

///////////////////////////////////////////////////////////////////////

[<Sealed; AbstractClass>]
type public AlgorithmW =

    static member infer (env:TypeEnv) expr =
        let state = new InferState()
        let (s, t) = AlgorithmW.infer env expr state
        apply t s

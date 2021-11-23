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
module private AlgorithmM =

    let rec infer (env:TypeEnv) expr (rho:Type) (state:InferState) =
        match expr with
        
        // M(Γ,x,ρ) = U(ρ,{β/α}τ)
        //            where Γ(x) = ∀α.τ, new β
        | EVar name ->
            match Map.tryFind name env with
            | Some sigma ->
                let typ = instantiate sigma state
                mgu rho typ state
            | _ ->
                raise (new UnboundVariableException(name))
                
        // M(Γ,(),ρ) = U(ρ,ι)
        | ELit literal ->
            match literal with
            | LInt _ ->
                mgu rho TInt state
            | LBool _ ->
                mgu rho TBool state
                
        // M(Γ,λx.e,ρ) =
        //    let S1 = U(ρ,β1 → β2), new β1,β2
        //        S2 = M(S1 Γ+x:(S1 β1),e,S1 β2)
        //    in S2 S1
        | EAbs(name, expr) ->
            let itv = newTyVar "a" state
            let otv = newTyVar "a" state
            let s1 = mgu rho (TFun(itv, otv)) state
            let env' = Map.add name (Scheme.ofType (apply itv s1)) env
            let s2 = infer (apply env' s1) expr (apply otv s1) state
            composeSubst s2 s1
            
        // M(Γ,e1 e2,ρ) =
        //     let S1 = M(Γ,e1,β → ρ), new β
        //         S2 = M(S1 Γ,e2,S1 β)
        //     in S2 S1
        | EApp(func, arg) ->
            let tv = newTyVar "a" state
            let s1 = infer env func (TFun(tv, rho)) state
            let s2 = infer (apply env s1) arg (apply tv s1) state
            composeSubst s2 s1
            
        // M(Γ,let x = e1 in e2,ρ) =
        //     let S1 = M(Γ,e1,β), new β
        //         S2 = M(S1 Γ+x:Clos(S1 Γ,S1 β),e2,S1 ρ)
        //     in S2 S1
        | ELet(name, expr, body) ->
            let tv = newTyVar "a" state
            let s1 = infer env expr tv state
            let s' = generalize (apply env s1) (apply tv s1)
            let env' = Map.add name s' env
            let s2 = infer (apply env' s1) body (apply rho s1) state
            composeSubst s2 s1
            
        // M(Γ,fix f λx.e,ρ) = M(Γ+f:ρ,λx:e,ρ)
        | EFix(func, name, body) ->
            let env'' = Map.add func (Scheme.ofType rho) env
            infer env'' (EAbs(name, body)) rho state

///////////////////////////////////////////////////////////////////////

[<Sealed; AbstractClass>]
type public AlgorithmM =

    static member infer (env:TypeEnv) expr =
        let state = new InferState()
        let tv = newTyVar "a" state
        let s = AlgorithmM.infer env expr tv state
        apply tv s

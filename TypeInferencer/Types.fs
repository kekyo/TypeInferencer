////////////////////////////////////////////////////////////////////////////
//
// Type inference implementation both Algorithm W and Algorithm M in F#
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace TypeInferencer

open System.Diagnostics

///////////////////////////////////////////////////////////////////////

type IPrintable =
    abstract friendly : string
    abstract strict : string

///////////////////////////////////////////////////////////////////////

[<DebuggerDisplay("{friendly}")>]
type public Lit =
    | LInt of value:int32
    | LBool of value:bool
    member this.friendly =
        match this with
        | LInt value ->
            value.ToString()
        | LBool value ->
            value.ToString()
    member this.strict =
        match this with
        | LInt value ->
            $"LInt {value}"
        | LBool value ->
            $"LBool {value}"
    override this.ToString() =
        this.friendly
    interface IPrintable with
        member this.friendly =
            this.friendly
        member this.strict =
            this.strict

///////////////////////////////////////////////////////////////////////

[<DebuggerDisplay("{friendly}")>]
type public Exp =
    | EVar of name:string
    | ELit of literal:Lit
    | EApp of func:Exp * arg:Exp
    | EAbs of name:string * expr:Exp
    | ELet of name:string * expr:Exp * body:Exp
    | EFix of func:string * name:string * expr:Exp
    member this.friendly =
        match this with
        | EVar name ->
            name
        | ELit literal ->
            literal.ToString()
        | EApp (func, arg) ->
            match arg with
            | EApp _
            | EAbs _
            | ELet _
            | EFix _ ->
                $"{func.friendly} ({arg.friendly})"
            | _ ->
                $"{func.friendly} {arg.friendly}"
        | EAbs (name, expr) ->
            $"fun {name} -> {expr.friendly}"
        | ELet (name, expr, body) ->
            $"let {name} = {expr.friendly} in {body.friendly}"
        | EFix (func, name, expr) ->
            $"fix {func} ({EAbs(name, expr).friendly})"
    member this.strict =
        this.ToString()
    interface IPrintable with
        member this.friendly =
            this.friendly
        member this.strict =
            this.strict

///////////////////////////////////////////////////////////////////////

[<DebuggerDisplay("{friendly}")>]
type public Type =
    | TVar of name:string
    | TInt
    | TBool
    | TFun of parameterType:Type * resultType:Type
    member this.friendly =
        match this with
        | TVar name ->
            name
        | TInt ->
            "Int"
        | TBool ->
            "Bool"
        | TFun (parameterType, resultType) ->
            match parameterType with
            | TFun _ ->
                $"({parameterType.friendly}) -> {resultType.friendly}"
            | _ ->
                $"{parameterType.friendly} -> {resultType.friendly}"
    member this.strict =
        this.ToString()
    interface IPrintable with
        member this.friendly =
            this.friendly
        member this.strict =
            this.strict

///////////////////////////////////////////////////////////////////////

[<DebuggerDisplay("{friendly}")>]
type public Scheme =
    | Scheme of vars:Set<string> * typ:Type
    static member inline ofType typ =
        Scheme(Set.empty, typ)
    member this.friendly =
        match this with
        | Scheme (vars, typ) ->
            sprintf "All %s . %s"
                (System.String.Join(",", vars))
                (typ.ToString())
    member this.strict =
        this.ToString()
    interface IPrintable with
        member this.friendly =
            this.friendly
        member this.strict =
            this.strict

type public Subst =
    Map<string, Type>

type public TypeEnv =
    Map<string, Scheme>

///////////////////////////////////////////////////////////////////////

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<AutoOpen>]
module public Printer =

    let inline show (printable:IPrintable) =
        printable.friendly
    let inline showStrict (printable:IPrintable) =
        printable.strict

///////////////////////////////////////////////////////////////////////

[<AbstractClass>]
type TypeInferenceException(message) =
    inherit System.Exception(message)

[<Sealed>]
type UnificationException(type1:Type, type2:Type) =
    inherit TypeInferenceException($"types do not unify: {type1.friendly} vs. {type2.friendly}")
    member _.Type1 =
        type1
    member _.Type2 =
        type2

[<Sealed>]
type OccurrenceException(name: string, typ:Type) =
    inherit TypeInferenceException($"occur check fails: {name} vs. {typ.friendly}")
    member _.Name =
        name
    member _.Type =
        typ

[<Sealed>]
type UnboundVariableException(name: string) =
    inherit TypeInferenceException($"unbound variable: {name}")
    member _.Name =
        name

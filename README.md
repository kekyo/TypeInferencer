# Algorithm W and Algorithm M in F#

[![Project Status: Active – The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#active)

[![NuGet TypeInferencer](https://img.shields.io/nuget/v/TypeInferencer.svg?style=flat)](https://www.nuget.org/packages/TypeInferencer)
[![CI build (main)](https://github.com/kekyo/TypeInferencer/workflows/.NET/badge.svg?branch=main)](https://github.com/kekyo/TypeInferencer/actions?query=branch%3Amain)

## What is this?

This is a type inference implementation of both `Algorithm W` and `Algorithm M` written in F#.

Referenced articles:

1. [`Algorithm W Step by Step`](http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.65.7733&rep=rep1&type=pdf)
2. [`Proofs about a Folklore Let-Polymorphic Type
Inference Algorithm`](https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.29.4595&rep=rep1&type=pdf)

The method of article 1 was implemented with care not to change it as much as possible.

### Example

```fsharp
// NuGet package is available.
#r "nuget: TypeInferencer"

open TypeInferencer

// `let id = fun x -> x in id id`
let expr =
    ELet("id",
        EAbs("x", EVar "x"),
        EApp(EVar "id", EVar "id"))

// Type environment (is empty)
let env = TypeEnv []

// Do inferring with `Algorithm W` (top-down)
let actual = infer TopDown env expr

// `a -> a`
System.Diagnostics.Debug.Assert(
    match actual with
    | TFun(TVar n1, TVar n2) when n1 = n2 -> true
    | _ -> false
)

// Pretty printing
printfn "Expression: %s" (show expr)
printfn "Actual: %s" (show actual)
```

### Well-defined types

AST expression type:

```fsharp
type public Lit =
    | LInt of value:int32
    | LBool of value:bool

type public Exp =
    | EVar of name:string
    | ELit of literal:Lit
    | EApp of func:Exp * arg:Exp
    | EAbs of name:string * expr:Exp
    | ELet of name:string * expr:Exp * body:Exp
    | EFix of func:string * name:string * expr:Exp
```

Result type type:

```fsharp
type public Type =
    | TVar of name:string
    | TInt
    | TBool
    | TFun of parameterType:Type * resultType:Type
```

### Requirements

* F# 6.0 or upper
* NuGet package supported platforms:
  * net6.0
  * net5.0
  * netcoreapp3.1
  * netcoreapp2.1
  * netstandard2.1
  * netstandard2.0
  * net48
  * net461

### License

Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo2)

License under Apache-v2.

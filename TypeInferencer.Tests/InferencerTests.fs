////////////////////////////////////////////////////////////////////////////
//
// Type inference implementation both Algorithm W and Algorithm M in F#
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace TypeInferencer

open NUnit.Framework

open type Inferencer  // Uses type instead module, because `TestCase` attribute couldn't receive union case value.
open type Assert

type InferencerTests() =

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e0 algorithm =
        // let id = fun x -> x
        // in
        //   id
        let expr =
            ELet("id",
                EAbs("x", EVar "x"),
                EVar "id")
        let env = TypeEnv([])
        let actual = infer algorithm env expr

        // a -> a
        IsTrue(
            match actual with
            | TFun(TVar n1, TVar n2) when n1 = n2 -> true
            | _ -> false
        ) |> ignore

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e1 algorithm =
        // let id = fun x -> x
        // in
        //   id id
        let expr =
            ELet("id",
                EAbs("x", EVar "x"),
                EApp(EVar "id", EVar "id"))
        let env = TypeEnv([])
        let actual = infer algorithm env expr

        // a -> a
        IsTrue(
            match actual with
            | TFun(TVar n1, TVar n2) when n1 = n2 -> true
            | _ -> false
        ) |> ignore

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e2 algorithm =
        // let id = fun x ->
        //   let y = x
        //   in
        //     y
        // in
        //   id id
        let expr =
            ELet("id",
                EAbs("x",
                     ELet("y",
                          EVar "x",
                          EVar "y")),
                EApp(EVar "id", EVar "id"))
        let env = TypeEnv([])
        let actual = infer algorithm env expr

        // a -> a
        IsTrue(
            match actual with
            | TFun(TVar n1, TVar n2) when n1 = n2 -> true
            | _ -> false
        ) |> ignore

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e3 algorithm =
        // let id = fun x ->
        //   let y = x
        //   in
        //     y
        // in
        //   id id 2
        let expr =
            ELet("id",
                EAbs("x",
                     ELet("y",
                          EVar "x",
                          EVar "y")),
                EApp(
                    EApp(EVar "id", EVar "id"),
                    ELit(LInt 2)))
        let env = TypeEnv([])
        let actual = infer algorithm env expr

        // TInt
        let expected = TInt
        AreEqual(expected, actual) |> ignore

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e4 algorithm =
        // let id = fun x ->
        //   x x
        // in
        //   id
        let expr =
            ELet("id",
                 EAbs("x",
                      EApp(EVar "x", EVar "x")),
                 EVar "id")
        let env = TypeEnv([])

        try
            let actual = infer algorithm env expr
            Fail()
        with
        | :? OccurrenceException as ex ->
            IsTrue(
                match ex.Type with
                | TFun(TVar pn, TVar rn) when ((pn <> rn) && ((ex.Name = pn) || (ex.Name = rn))) -> true
                | _ -> false
            ) |> ignore
        | _ ->
            reraise()

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e5 algorithm =
        // fun m ->
        //   let y = m
        //   in
        //     let x = y true
        //     in
        //       x
        let expr =
            EAbs("m",
                 ELet("y",
                      EVar "m",
                      ELet("x",
                           EApp(EVar "y", ELit(LBool true)),
                           EVar "x")))
        let env = TypeEnv([])
        let actual = infer algorithm env expr

        // (bool -> a) -> a
        IsTrue(
            match actual with
            | TFun(TFun(TBool, TVar n1), TVar n2) when n1 = n2 -> true
            | _ -> false
        ) |> ignore

    [<TestCase(Algorithm.TopDown)>]
    [<TestCase(Algorithm.BottomUp)>]
    member _.e6 algorithm =
        // fix id (fun x -> id x)
        let expr =
            EFix("id",
                 "x",
                 EApp(EVar "id", EVar "x"))
        let env = TypeEnv([])
        let actual = infer algorithm env expr

        // a1 -> a2
        IsTrue(
            match actual with
            | TFun(TVar n1, TVar n2) when n1 <> n2 -> true
            | _ -> false
        ) |> ignore

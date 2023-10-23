#!/bin/sh

# Type inference implementation both Algorithm W and Algorithm M in F#
# Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo2)
#
# Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo ""
echo "==========================================================="
echo "Build"
echo ""

# git clean -xfd

dotnet build -p:Configuration=Release -p:Platform="Any CPU" -p:RestoreNoCache=True TypeInferencer.sln
dotnet pack -p:Configuration=Release -p:Platform="Any CPU" -o artifacts TypeInferencer.sln

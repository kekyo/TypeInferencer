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

if [ -e artifacts ]; then
    rm -rf artifacts
fi

mkdir artifacts

dotnet restore
dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts TypeInferencer/TypeInferencer.fsproj

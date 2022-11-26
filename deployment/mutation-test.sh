#!/bin/bash
set -e

./deployment/build.sh
dotnet tool install -g dotnet-stryker --version 3.2.0

pushd ./source/Gnomeshade.WebApi
dotnet stryker --reporter "dashboard" --reporter "progress" --dashboard-api-key "$STRYKER_API_KEY"
popd

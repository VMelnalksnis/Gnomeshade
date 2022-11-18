#!/bin/bash
set -e

./deployment/restore.sh
dotnet build --configuration Release --no-restore /warnAsError /nologo /clp:NoSummary

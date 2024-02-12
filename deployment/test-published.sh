#!/bin/sh
set -e

version=$(tr -d '[:space:]' <version)

./deployment/restore.sh

dotnet publish \
	./tests/"$1"/"$1".csproj \
	--runtime "$2" \
	--configuration Release \
	--self-contained \
	--no-restore \
	-p:AssemblyVersion="$version.$3" \
	-p:InformationalVersion="$version$4$2" \
	-p:PublishSingleFile=false \
	/warnAsError \
	/nologo

dotnet test ./tests/"$1"/bin/Release/net8.0/"$2"/publish/"$1".dll

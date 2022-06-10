#!/bin/bash
dotnet publish \
./source/"$1"/"$1".csproj \
--runtime "$2" \
--configuration Release \
--self-contained \
-p:PublishSingleFile=true \
-p:AssemblyVersion="$3"."$4" \
-p:InformationalVersion="$3""$5"+"$2" \
/warnAsError \
/nologo

pushd "./source/$1/bin/Release/net6.0/$2/publish" || exit
zip -r -9 "$1"_"$2".zip .
popd || exit

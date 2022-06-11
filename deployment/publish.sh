#!/bin/bash
version=$(cat version)
publish_dir="./source/$1/bin/Release/net6.0/$2/publish"
archive_name="$1_$2.zip"

dotnet publish \
	./source/"$1"/"$1".csproj \
	--runtime "$2" \
	--configuration Release \
	--self-contained \
	-p:PublishSingleFile=true \
	-p:AssemblyVersion="$version"."$3" \
	-p:InformationalVersion="$version""$4"+"$2" \
	/warnAsError \
	/nologo

pushd "$publish_dir" || exit
zip -r -9 "$archive_name" .
popd || exit

echo "::set-output name=artifact-name::$archive_name"
echo "::set-output name=artifact::$publish_dir/$archive_name"

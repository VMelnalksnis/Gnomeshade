#!/bin/sh
set -e

version=$(tr -d '[:space:]' <version)
publish_dir="./source/$1/bin/Release/net7.0/$2/publish"
archive_name="$1_$2.zip"

./deployment/restore.sh

dotnet publish \
	./source/"$1"/"$1".csproj \
	--runtime "$2" \
	--configuration Release \
	--self-contained \
	--no-restore \
	-p:PublishSingleFile=true \
	-p:AssemblyVersion="$version.$3" \
	-p:InformationalVersion="$version$4$2" \
	/warnAsError \
	/nologo

if [ -z "$GITHUB_OUTPUT" ]; then
	echo "Not in GitHub Actions"
	exit 0
fi

(
	cd "$publish_dir" || exit
	zip -r -9 "$archive_name" .
)

echo "artifact-name=$archive_name" >>"$GITHUB_OUTPUT"
echo "artifact=$publish_dir/$archive_name" >>"$GITHUB_OUTPUT"

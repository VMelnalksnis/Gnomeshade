#!/bin/bash
version=$(cat version)
publish_dir="./source/$1/bin/Release"
full_version="$version.$2"
package_name="$1.$version.nupkg"
symbols_name="$1.$version.snupkg"

dotnet pack \
	./source/"$1"/"$1".csproj \
	--configuration Release \
	--no-restore \
	-p:AssemblyVersion="$full_version" \
	-p:AssemblyFileVersion="$full_version" \
	-p:PackageVersion="$version" \
	-p:InformationalVersion="$version""$3" \
	/warnAsError \
	/nologo ||
	exit

echo "::set-output name=artifact-name::$package_name"
echo "::set-output name=artifact::$publish_dir/$package_name"

echo "::set-output name=symbols-name::$symbols_name"
echo "::set-output name=symbols::$publish_dir/$symbols_name"

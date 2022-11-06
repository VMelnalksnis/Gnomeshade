#!/bin/bash
version=$(cat version)
publish_dir="./source/$1/bin/Release"
full_version="$version.$2"
if [[ $3 == refs/tags/* ]]; then
	package_version="$version"
else
	package_version="$version-nightly.$2"
fi
package_name="$1.$package_version.nupkg"
symbols_name="$1.$package_version.snupkg"

dotnet pack \
	./source/"$1"/"$1".csproj \
	--configuration Release \
	--no-restore \
	-p:AssemblyVersion="$full_version" \
	-p:AssemblyFileVersion="$full_version" \
	-p:PackageVersion="$package_version" \
	-p:InformationalVersion="$version""$3" \
	/warnAsError \
	/nologo ||
	exit

echo "artifact-name=$package_name" >>"$GITHUB_OUTPUT"
echo "artifact=$publish_dir/$package_name" >>"$GITHUB_OUTPUT"

echo "symbols-name=$symbols_name" >>"$GITHUB_OUTPUT"
echo "symbols=$publish_dir/$symbols_name" >>"$GITHUB_OUTPUT"

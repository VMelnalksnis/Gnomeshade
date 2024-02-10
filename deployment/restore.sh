#!/bin/sh
set -e

if [ -z "$1" ]; then
	echo "Restoring solution"
	dotnet restore --locked-mode /p:Configuration="Release"
else
	echo "Restoring project $1"
	dotnet restore ./source/"$1"/"$1".csproj --locked-mode /p:Configuration="Release"
fi

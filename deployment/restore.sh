#!/bin/sh
set -e

dotnet restore --locked-mode /p:Configuration="Release"

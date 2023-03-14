#!/bin/bash
set -e

./deployment/restore.sh

packages=$(dotnet list package --include-transitive --vulnerable --format json || true)
echo "$packages"

if [[ "$packages" == *"vulnerabilities"* ]]; then
	echo "Vulnerable packages"
	exit 1
elif [[ "$packages" != *"projects"* ]]; then
	echo "Unexpected output"
	exit 1
fi

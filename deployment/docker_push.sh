#!/bin/sh
set -e

build_number=$1
version=${2-$(tr -d '[:space:]' <version)}

docker build \
	--tag ghcr.io/vmelnalksnis/gnomeshade:"$version" \
	--build-arg "BUILD_NUMBER=$build_number" \
	./

docker push \
	ghcr.io/vmelnalksnis/gnomeshade:"$version"

#!/bin/bash
set -e

tag="ghcr.io/vmelnalksnis/gnomeshade:test"
name="test"

function printLogs() {
	docker logs $name
}

trap printLogs EXIT

docker build --tag $tag --build-arg "BUILD_NUMBER=$1" ./
docker run --name $name -d -p 8000:8080 -e "Admin__Password=$2" -e "GNOMESHADE_DEMO=true" $tag

wget --tries=10 --retry-connrefused --waitretry=1 --timeout=15 "http://localhost:8000/api/v1.0/health"

if [[ $(cat health) != "Healthy" ]]
then
	exit 1
fi

http_response=$(curl --header "Content-Type: application/json" \
                	--request POST \
                	--data '{"username":"demo", "password": "Demo1!" }' \
                	--output response \
                	--write-out "%{response_code}" \
                	--verbose \
                	http://localhost:8000/api/v1.0/Authentication/Login)
cat response

if [[ $http_response != "200" ]]
then
    exit 1
fi

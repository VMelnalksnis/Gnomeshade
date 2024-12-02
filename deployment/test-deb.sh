#!/bin/bash
set -e

name="test"

function printLogs() {
	docker logs $name
	docker exec $name service --status-all
	docker exec $name cat /var/log/gnomeshade
}

trap printLogs EXIT

cp "$1" ./gnomeshade.deb

docker build --tag devuan:test ./
docker run --detach --name $name --publish 5000:5000 \
	devuan:test /bin/bash -c \
	"while true; do sleep 2; done"

url="http://[::]:5000"
cat > ./appsettings.Production.json<< EOF
{
	"Kestrel": {
		"Endpoints": {
			"Http": {
				"Url": "$url"
			}
		}
	},
    "Admin": {
    	"Password": "$2"
    },
	"ConnectionStrings": {
		"Gnomeshade": "Data Source=/opt/gnomeshade/gnomeshade.db"
	},
	"Database": {
		"Provider": "Sqlite"
	},
    "Jwt": {
    	"ValidAudience": "$url",
    	"ValidIssuer": "$url",
    	"Secret": "00000000000000000000000000000000"
    },
	"GNOMESHADE_DEMO": true
}
EOF

docker cp ./appsettings.Production.json $name:/etc/opt/gnomeshade/appsettings.Production.json
docker exec $name service gnomeshade start

wget --tries=10 --retry-connrefused --waitretry=1 --timeout=15 "http://localhost:5000/api/v1.0/health"

if [[ $(cat health) != "Healthy" ]]; then
	exit 1
fi

http_response=$(curl --header "Content-Type: application/json" \
                	--request POST \
                	--data '{"username":"demo", "password": "Demo1!" }' \
                	--output response \
                	--write-out "%{response_code}" \
                	--verbose \
                	http://localhost:5000/api/v1.0/Authentication/Login)
cat response

if [[ $http_response != "200" ]]
then
    exit 1
fi

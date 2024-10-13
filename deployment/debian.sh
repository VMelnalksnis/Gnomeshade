#!/bin/bash
set -e

sudo apt-get update
sudo apt-get install lintian moreutils -y

archive_path="$1"
version=$(cat version)
full_version="$version.$2"
maintainer_email="valters.melnalksnis@gnomeshade.org"
maintainer="Valters Melnalksnis <$maintainer_email>"

mkdir -p ./deployment/debian/gnomeshade/opt/gnomeshade
unzip "$archive_path" -d ./deployment/debian/gnomeshade/opt/gnomeshade

cd ./deployment/debian/
mv gnomeshade/opt/gnomeshade/appsettings.json gnomeshade/etc/opt/gnomeshade/appsettings.json

chmod +x gnomeshade/opt/gnomeshade/Gnomeshade.WebApi
chmod 0755 gnomeshade/opt/gnomeshade/libe_sqlite3.so
strip gnomeshade/opt/gnomeshade/libe_sqlite3.so

export FULL_VERSION=$full_version
export MAINTAINER=$maintainer
export MAINTAINER_EMAIL=$maintainer_email

envsubst < gnomeshade/DEBIAN/control | sponge gnomeshade/DEBIAN/control
envsubst < gnomeshade/usr/share/doc/gnomeshade/copyright | sponge gnomeshade/usr/share/doc/gnomeshade/copyright
envsubst < gnomeshade/usr/share/doc/gnomeshade/changelog | gzip --no-name --best > "gnomeshade/usr/share/doc/gnomeshade/changelog.gz"
rm gnomeshade/usr/share/doc/gnomeshade/changelog

dpkg-deb --root-owner-group --build gnomeshade

# unstripped-binary-or-object suppressed because gnomeshade/opt/gnomeshade/Gnomeshade.WebApi
# cannot be stripped without corrupting the application
lintian \
	--suppress-tags dir-or-file-in-opt,dir-or-file-in-etc-opt \
	--suppress-tags unstripped-binary-or-object \
	gnomeshade.deb

cd ../../
mv ./deployment/debian/gnomeshade.deb ./gnomeshade.deb

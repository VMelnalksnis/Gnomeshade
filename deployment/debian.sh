#!/bin/bash
set -e

sudo apt update
sudo apt install lintian -y

archive_path="$1"
version=$(cat version)
full_version="$version.$2"
changelog_path="gnomeshade/usr/share/doc/gnomeshade/changelog.gz"
maintainer_email="valters.melnalksnis@gnomeshade.org"
maintainer="Valters Melnalksnis <$maintainer_email>"
time="Fri, 24 Jun 2022 19:28:01 +0200"

mkdir -p gnomeshade/opt/gnomeshade
unzip "$archive_path" -d gnomeshade/opt/gnomeshade

chmod +x gnomeshade/opt/gnomeshade/Gnomeshade.WebApi

chmod 0755 gnomeshade/opt/gnomeshade/libe_sqlite3.so
strip gnomeshade/opt/gnomeshade/libe_sqlite3.so

mkdir -p gnomeshade/DEBIAN
cp deployment/debian/postinst gnomeshade/DEBIAN/postinst
cp deployment/debian/prerm gnomeshade/DEBIAN/prerm
cp deployment/debian/postrm gnomeshade/DEBIAN/postrm

export FULL_VERSION=$full_version
export MAINTAINER=$maintainer
envsubst <deployment/debian/control >gnomeshade/DEBIAN/control

mkdir -p gnomeshade/etc/opt/gnomeshade
mv gnomeshade/opt/gnomeshade/appsettings.json gnomeshade/etc/opt/gnomeshade/appsettings.json
echo "/etc/opt/gnomeshade/appsettings.json" >>gnomeshade/DEBIAN/conffiles

mkdir -p gnomeshade/usr/share/doc/gnomeshade
export MAINTAINER_EMAIL=$maintainer_email
envsubst <deployment/debian/copyright >gnomeshade/usr/share/doc/gnomeshade/copyright

export CHANGELOG_TIME=$time
envsubst <deployment/debian/changelog >changelog
cat changelog

gzip -n --best changelog
mv changelog.gz $changelog_path

mkdir -p gnomeshade/lib/systemd/system
cp deployment/debian/gnomeshade.service gnomeshade/lib/systemd/system/gnomeshade.service

mkdir -p gnomeshade/etc/init.d
cp deployment/debian/gnomeshade gnomeshade/etc/init.d/gnomeshade
echo "/etc/init.d/gnomeshade" >>gnomeshade/DEBIAN/conffiles

dpkg-deb --root-owner-group --build gnomeshade

# unstripped-binary-or-object suppressed because gnomeshade/opt/gnomeshade/Gnomeshade.WebApi
# cannot be stripped without corrupting the application
lintian \
	--suppress-tags dir-or-file-in-opt,dir-or-file-in-etc-opt \
	--suppress-tags unstripped-binary-or-object \
	gnomeshade.deb

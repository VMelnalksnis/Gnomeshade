#!/bin/bash
set -e

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

pushd gnomeshade
dpkg-shlibdeps -v -e gnomeshade/opt/gnomeshade/Gnomeshade.WebApi -e gnomeshade/opt/gnomeshade/libe_sqlite3.so
popd

objcopy --strip-debug --strip-unneeded gnomeshade/opt/gnomeshade/libe_sqlite3.so

mkdir -p gnomeshade/DEBIAN
cp deployment/debian/postinst gnomeshade/DEBIAN/postinst
cp deployment/debian/prerm gnomeshade/DEBIAN/prerm

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

dpkg-deb --root-owner-group --build gnomeshade
lintian --suppress-tags dir-or-file-in-opt,dir-or-file-in-etc-opt,unstripped-binary-or-object gnomeshade.deb

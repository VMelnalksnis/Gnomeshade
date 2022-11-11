#!/bin/bash

archive_path="$1"
version=$(cat version)
full_version="$version.$2"
changelog_path="gnomeshade/usr/share/doc/gnomeshade/changelog.gz"
maintainer_email="valters.melnalksnis@gnomeshade.org"
maintainer="Valters Melnalksnis <$maintainer_email>"
time="Fri, 24 Jun 2022 19:28:01 +0200"

mkdir -p gnomeshade/opt/gnomeshade || exit
unzip "$archive_path" -d gnomeshade/opt/gnomeshade || exit
chmod +x gnomeshade/opt/gnomeshade/Gnomeshade.WebApi || exit
objcopy --strip-debug --strip-unneeded gnomeshade/opt/gnomeshade/libe_sqlite3.so || exit

mkdir -p gnomeshade/DEBIAN || exit
cp deployment/debian/postinst gnomeshade/DEBIAN/postinst || exit
cp deployment/debian/prerm gnomeshade/DEBIAN/prerm || exit

export FULL_VERSION=$full_version
export MAINTAINER=$maintainer
envsubst <deployment/debian/control >gnomeshade/DEBIAN/control || exit

mkdir -p gnomeshade/etc/opt/gnomeshade || exit
mv gnomeshade/opt/gnomeshade/appsettings.json gnomeshade/etc/opt/gnomeshade/appsettings.json
echo "/etc/opt/gnomeshade/appsettings.json" >>gnomeshade/DEBIAN/conffiles || exit

mkdir -p gnomeshade/usr/share/doc/gnomeshade || exit
export MAINTAINER_EMAIL=$maintainer_email || exit
envsubst <deployment/debian/copyright >gnomeshade/usr/share/doc/gnomeshade/copyright || exit

export CHANGELOG_TIME=$time
envsubst <deployment/debian/changelog >changelog || exit
cat changelog || exit

gzip -n --best changelog || exit
mv changelog.gz $changelog_path || exit

mkdir -p gnomeshade/lib/systemd/system || exit
cp deployment/debian/gnomeshade.service gnomeshade/lib/systemd/system/gnomeshade.service || exit

dpkg-deb --root-owner-group --build gnomeshade || exit
lintian --suppress-tags dir-or-file-in-opt,dir-or-file-in-etc-opt,unstripped-binary-or-object gnomeshade.deb || exit

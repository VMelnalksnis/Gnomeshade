#!/bin/bash

sudo apt update && sudo apt install lintian || exit

archive_path="$1"
version=$(cat version)
full_version="$version.$2"
control_path="gnomeshade/DEBIAN/control"
copyright_path="gnomeshade/usr/share/doc/gnomeshade/copyright"
changelog_path="gnomeshade/usr/share/doc/gnomeshade/changelog.gz"
maintainer_email="valters.melnalksnis@gnomeshade.org"
maintainer="Valters Melnalksnis <$maintainer_email>"
time="Fri, 24 Jun 2022 19:28:01 +0200"

mkdir -p gnomeshade/opt/gnomeshade || exit
unzip "$archive_path" -d gnomeshade/opt/gnomeshade || exit

mkdir -p gnomeshade/DEBIAN || exit
echo "Package: gnomeshade" >>$control_path || exit
echo "Version: $full_version" >>$control_path || exit
echo "Section: misc" >>$control_path || exit
echo "Priority: optional" >>$control_path || exit
echo "Architecture: all" >>$control_path || exit
echo "Maintainer: $maintainer" >>$control_path || exit
echo "Description: Placeholder" >>$control_path || exit
echo " Placeholder" >>$control_path || exit
echo "Homepage: https://gnomeshade.org" >>$control_path || exit

mkdir -p gnomeshade/etc/opt/gnomeshade || exit
mv gnomeshade/opt/gnomeshade/appsettings.json gnomeshade/etc/opt/gnomeshade/appsettings.json
echo "/etc/opt/gnomeshade/appsettings.json" >>gnomeshade/DEBIAN/conffiles || exit

mkdir -p gnomeshade/usr/share/doc/gnomeshade || exit
echo "Format: http://www.debian.org/doc/packaging-manuals/copyright-format/1.0/" >>$copyright_path || exit
echo "Upstream-Name: gnomeshade" >>$copyright_path || exit
echo "Upstream-Contact: $maintainer_email" >>$copyright_path || exit
echo "Source: https://github.com/VMelnalksnis/Gnomeshade" >>$copyright_path || exit
echo "" >>$copyright_path || exit
echo "Files: *" >>$copyright_path || exit
echo "Copyright: 2021 $maintainer" >>$copyright_path || exit
echo "License: APGL-3.0-or-later" >>$copyright_path || exit

echo "gnomeshade ($full_version) stable; urgency=low" >>changelog || exit
echo "" >>changelog || exit
echo "  * Placeholder" >>changelog || exit
echo "" >>changelog || exit
echo " -- $maintainer  $time" >>changelog || exit

gzip -n --best changelog || exit
mv changelog.gz $changelog_path || exit

dpkg-deb --root-owner-group --build gnomeshade || exit
lintian --suppress-tags dir-or-file-in-opt,dir-or-file-in-etc-opt gnomeshade.deb || exit

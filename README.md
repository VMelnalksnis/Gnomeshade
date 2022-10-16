[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![codecov](https://codecov.io/gh/VMelnalksnis/Gnomeshade/branch/master/graph/badge.svg?token=5GWIPI74DO)](https://codecov.io/gh/VMelnalksnis/Gnomeshade)
[![build and test](https://github.com/VMelnalksnis/Gnomeshade/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/VMelnalksnis/Gnomeshade/actions/workflows/build-and-test.yml)
[![NuGet vulnerabilities](https://github.com/VMelnalksnis/Gnomeshade/actions/workflows/nuget-vulnerabilities.yml/badge.svg)](https://github.com/VMelnalksnis/Gnomeshade/actions/workflows/nuget-vulnerabilities.yml)

# Gnomeshade

A free, open source and self-hosted personal finance manager.

## Getting Started

> ⚠️ WARNING: **NOT READY FOR PRODUCTION!**. This project is under heavy development, there will be continuous functions, features and api changes.

Currently all applications are publish as self-contained, so they can be run after extracting and editing the
appsettings.json file.
The latest release can be found [here](https://github.com/VMelnalksnis/Gnomeshade/releases/latest).
The latest build results are added to
the [nightly release](https://github.com/VMelnalksnis/Gnomeshade/releases/tag/nightly).

### Server

* A debian package is provided in releases, which can be installed using `sudo dpkg -i gnomeshade.deb`
* The server requires a database, currently the following are supported:
	* SQLite
	* PostgreSQL (for [supported versions](https://www.postgresql.org/support/versioning/) see [integration tests](tests/Gnomeshade.WebApi.Tests.Integration.PostgreSQL/WebserverSetup.cs))
* For additional web server setup,
  see [Microsoft docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-6.0)
* To automatically import data with Nordigen,
  see [Nordigen docs](https://nordigen.com/en/account_information_documenation/integration/quickstart_guide/)
  and [configuration](https://github.com/VMelnalksnis/NordigenDotNet#usage)

### Client

An `.msi` installer is provided for the windows client application.

## Contributing

Instructions for contributing can be found in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This work [is licensed](LICENSE.txt) under the
[GNU Affero General Public License v3.0 or later](https://www.gnu.org/licenses/agpl-3.0.html).

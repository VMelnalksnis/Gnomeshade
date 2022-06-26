[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![codecov](https://codecov.io/gh/VMelnalksnis/Gnomeshade/branch/master/graph/badge.svg?token=5GWIPI74DO)](https://codecov.io/gh/VMelnalksnis/Gnomeshade)

# Gnomeshade

A free, open source and self-hosted personal finance manager.

## Getting Started

Currently all applications are publish as self-contained, so they can be run after extracting and editing the
appsettings.json file.
The latest release can be found [here](https://github.com/VMelnalksnis/Gnomeshade/releases/latest). 
The latest build results are added to the [nightly release](https://github.com/VMelnalksnis/Gnomeshade/releases/tag/nightly).

### Server

* A debian package is provided, which can be installed using `sudo dpkg -i filename.deb`
* For additional web server setup,
  see [Microsoft docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-6.0)
* To automatically import data with Nordigen,
  see [Nordigen docs](https://nordigen.com/en/account_information_documenation/integration/quickstart_guide/)
  and [configuration](https://github.com/VMelnalksnis/NordigenDotNet#usage)

### Client

An `.msi` installer is provided for the windows client application.

Currently the client settings file must be edited manually [#82](https://github.com/VMelnalksnis/Gnomeshade/issues/82),
and can be found in the installation directory (`%LocalAppData%/Gnomeshade`).
An example of the configuration file can be found [here](source/Gnomeshade.Interfaces.Desktop/appsettings.json).
The appsettings.json file should not be edited directly,
but an appsettings.user.json file should be created.

## Contributing

Instructions for contributing can be found in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This work [is licensed](LICENSE.txt) under the
[GNU Affero General Public License v3.0 or later](https://www.gnu.org/licenses/agpl-3.0.html).

## Contact

You can contact me at [valters.melnalksnis@gnomeshade.org](mailto:valters.melnalksnis@gnomeshade.org).

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![codecov](https://codecov.io/gh/VMelnalksnis/Gnomeshade/branch/master/graph/badge.svg?token=5GWIPI74DO)](https://codecov.io/gh/VMelnalksnis/Gnomeshade)
[![Nightly Release](https://github.com/VMelnalksnis/Gnomeshade/actions/workflows/nightly.yml/badge.svg)](https://github.com/VMelnalksnis/Gnomeshade/actions/workflows/nightly.yml)

# Gnomeshade

A free, open source and self-hosted personal finance manager.

## Getting Started

Currently all applications are publish as self-contained, so they can be run after extracting and editing the
appsettings.json file.
The latest release can be found [here](https://github.com/VMelnalksnis/Gnomeshade/releases/latest).

### Server

* For ubuntu installation instructions see the [ansible task](deployment/ansible/gnomeshade_nightly/tasks/main.yml)
* For additional web server setup,
  see [Microsoft docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-6.0)
* To automatically import data with Nordigen,
  see [Nordigen docs](https://nordigen.com/en/account_information_documenation/integration/quickstart_guide/)
  and [configuration](https://github.com/VMelnalksnis/NordigenDotNet#usage)

### Client

Currently the client settings file must be edited manually [#82](https://github.com/VMelnalksnis/Gnomeshade/issues/82).
An example of the configuration file can be found [here](source/Gnomeshade.Interfaces.Desktop/appsettings.json).

## Contributing

Instructions for contributing can be found in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This work [is licensed](LICENSE.txt) under the
[GNU Affero General Public License v3.0 or later](https://www.gnu.org/licenses/agpl-3.0.html).

## Contact

You can contact me at [valters.melnalksnis@gnomeshade.org](mailto:valters.melnalksnis@gnomeshade.org).

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Reflection;

namespace Gnomeshade.WebApi.Services;

internal sealed class ApplicationVersionService
{
	private static readonly string _version = Assembly.GetExecutingAssembly().GetName().Version is { } version
		? $"v{version.Major}.{version.Minor}.{version.Revision}"
		: string.Empty;

	internal string Version => _version;
}

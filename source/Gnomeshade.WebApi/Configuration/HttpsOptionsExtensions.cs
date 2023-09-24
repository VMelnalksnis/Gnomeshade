// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace Gnomeshade.WebApi.Configuration;

internal static class HttpsOptionsExtensions
{
	internal static void ConfigureCipherSuites(
		this HttpsConnectionAdapterOptions options,
		WebHostBuilderContext context)
	{
		if (!context.Configuration.GetValidIfDefined<TlsOptions>(out var tlsOptions))
		{
			return;
		}

		options.OnAuthenticate = (_, sslAuthenticationOptions) =>
		{
			if (!OperatingSystem.IsWindows())
			{
				sslAuthenticationOptions.CipherSuitesPolicy = new(tlsOptions.CipherSuites);
			}
		};
	}
}

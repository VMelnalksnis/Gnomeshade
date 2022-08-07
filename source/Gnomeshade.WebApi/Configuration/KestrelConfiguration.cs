// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Gnomeshade.WebApi.Configuration;

internal static class KestrelConfiguration
{
	internal static void ConfigureOptions(WebHostBuilderContext context, KestrelServerOptions options)
	{
		options.ConfigureHttpsDefaults(httpsOptions =>
		{
			if (!context.Configuration.GetValidIfDefined<TlsOptions>(out var tlsOptions))
			{
				return;
			}

			httpsOptions.OnAuthenticate = (_, sslAuthenticationOptions) =>
			{
				sslAuthenticationOptions.CipherSuitesPolicy = new(tlsOptions.CipherSuites);
			};
		});
	}
}

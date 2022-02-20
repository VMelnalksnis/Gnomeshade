// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

internal static class KestrelConfiguration
{
	internal static void ConfigureOptions(WebHostBuilderContext context, KestrelServerOptions options)
	{
		var tlsOptions = context.Configuration.GetValid<TlsOptions>();

		options.ConfigureHttpsDefaults(httpsOptions =>
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}

			httpsOptions.OnAuthenticate = (_, sslAuthenticationOptions) =>
			{
				if (tlsOptions.CipherSuites is not null)
				{
					sslAuthenticationOptions.CipherSuitesPolicy = new(tlsOptions.CipherSuites);
				}
			};
		});
	}
}

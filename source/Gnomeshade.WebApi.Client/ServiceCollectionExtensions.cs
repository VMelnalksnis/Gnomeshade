// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gnomeshade.WebApi.Client;

/// <summary>Collection of methods for configuring <see cref="IGnomeshadeClient"/> in <see cref="IServiceCollection"/>.</summary>
public static class ServiceCollectionExtensions
{
	private static readonly ProductInfoHeaderValue _userAgent;

	static ServiceCollectionExtensions()
	{
		var assemblyName = typeof(IGnomeshadeClient).Assembly.GetName();
		var assemblyShortName = assemblyName.Name ?? assemblyName.FullName.Split(',').First();
		_userAgent = new(assemblyShortName, assemblyName.Version?.ToString());
	}

	/// <summary>Adds all required services for <see cref="IGnomeshadeClient"/>, excluding external dependencies.</summary>
	/// <param name="serviceCollection">The service collection in which to register the services.</param>
	/// <param name="configuration">The configuration to which to bind options models.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/> for the <see cref="HttpClient"/> used by <see cref="IGnomeshadeClient"/>.</returns>
	public static IHttpClientBuilder AddGnomeshadeClient(
		this IServiceCollection serviceCollection,
		IConfiguration configuration)
	{
		serviceCollection
			.AddOptions<GnomeshadeOptions>()
			.Bind(configuration.GetSection(GnomeshadeOptions.SectionName))
			.ValidateDataAnnotations();

		return serviceCollection
			.AddSingleton<GnomeshadeTokenCache>()
			.AddSingleton<GnomeshadeJsonSerializerOptions>()
			.AddTransient<TokenDelegatingHandler>()
			.AddHttpClient<IGnomeshadeClient, GnomeshadeClient>((provider, client) =>
			{
				var gnomeshadeOptions = provider.GetRequiredService<IOptionsSnapshot<GnomeshadeOptions>>();
				var uriBuilder = new UriBuilder(gnomeshadeOptions.Value.BaseAddress!) { Path = "api/v1.0/" };
				client.BaseAddress = uriBuilder.Uri;
				client.DefaultRequestVersion = HttpVersion.Version30;
				client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
			})
			.AddHttpMessageHandler<TokenDelegatingHandler>();
	}
}

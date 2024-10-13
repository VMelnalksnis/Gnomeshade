// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net.Http;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using VMelnalksnis.NordigenDotNet;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class GnomeshadeWebApplicationFactory : WebApplicationFactory<Startup>
{
	private readonly IConfiguration _configuration;
	private readonly Action<IServiceCollection>? _configureServices;

	public GnomeshadeWebApplicationFactory(
		IConfiguration configuration,
		Action<IServiceCollection>? configureServices = null)
	{
		_configuration = configuration;
		_configureServices = configureServices;
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder
			.AddConfiguration(_configuration)
			.AddInMemoryCollection([new("GNOMESHADE_DEMO", "true")]));

		builder.ConfigureServices(collection =>
		{
			collection.AddTransient<EntityRepository>();
			collection.AddSingleton<Lazy<HttpMessageHandler>>(_ => new(() => Server.CreateHandler()));
			collection.AddSingleton<INordigenClient, MockNordigenClient>();
			_configureServices?.Invoke(collection);
		});

		builder.UseContentRoot("../../../../../source/Gnomeshade.WebApi");
		return base.CreateHost(builder);
	}

	protected override void ConfigureClient(HttpClient client)
	{
		client.BaseAddress = new("https://localhost:5001/api/");
	}
}

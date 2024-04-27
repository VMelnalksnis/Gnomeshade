// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
		builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddConfiguration(_configuration));
		builder.ConfigureServices(collection =>
		{
			collection.AddTransient<EntityRepository>();
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

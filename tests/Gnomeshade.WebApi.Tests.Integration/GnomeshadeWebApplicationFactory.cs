// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net.Http;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class GnomeshadeWebApplicationFactory : WebApplicationFactory<Startup>
{
	private readonly IConfiguration _configuration;

	public GnomeshadeWebApplicationFactory(IConfiguration configuration)
	{
		_configuration = configuration;
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.WriteTo.NUnitOutput(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
			.CreateLogger();

		Server.PreserveExecutionContext = true;
		Server.AllowSynchronousIO = true;
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.UseSerilog();
		builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddConfiguration(_configuration));
		builder.ConfigureServices(collection => collection.AddTransient<EntityRepository>());
		return base.CreateHost(builder);
	}

	protected override void ConfigureClient(HttpClient client)
	{
		client.BaseAddress = new("https://localhost:5001/api/v1.0/");
	}
}

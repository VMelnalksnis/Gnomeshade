// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
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
			.MinimumLevel.Verbose()
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.CreateLogger();

		Server.PreserveExecutionContext = true;
		Server.AllowSynchronousIO = true;
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.UseSerilog();
		builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddConfiguration(_configuration));
		return base.CreateHost(builder);
	}
}

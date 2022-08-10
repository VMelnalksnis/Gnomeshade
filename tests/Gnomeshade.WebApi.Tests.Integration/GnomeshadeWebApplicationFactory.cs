﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class GnomeshadeWebApplicationFactory : WebApplicationFactory<Startup>
{
	private readonly string _connectionString;

	public GnomeshadeWebApplicationFactory(string connectionString)
	{
		_connectionString = connectionString;

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
		builder.ConfigureAppConfiguration((_, configurationBuilder) =>
		{
			configurationBuilder.AddUserSecrets<GnomeshadeWebApplicationFactory>(true, true);
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{ "ConnectionStrings:FinanceDb", _connectionString },
			});
		});

		return base.CreateHost(builder);
	}
}
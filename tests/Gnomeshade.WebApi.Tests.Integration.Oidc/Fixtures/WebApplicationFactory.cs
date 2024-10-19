// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net.Http;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public sealed class WebApplicationFactory : WebApplicationFactory<Startup>
{
	private readonly IConfiguration _configuration;
	private readonly int _port;

	public WebApplicationFactory(IConfiguration configuration, int port)
	{
		_configuration = configuration;
		_port = port;
	}

	/// <inheritdoc />
	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddConfiguration(_configuration));
		return base.CreateHost(builder);
	}

	/// <inheritdoc />
	protected override void ConfigureClient(HttpClient client)
	{
		client.BaseAddress = new($"https://localhost:{_port}/api/");
	}
}

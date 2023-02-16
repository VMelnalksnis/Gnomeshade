// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Configuration;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi;

/// <summary>The application entry point.</summary>
public static class Program
{
	/// <summary>The application entry point.</summary>
	/// <param name="args">The command-line arguments of the application.</param>
	public static void Main(string[] args)
	{
		var logger = CreateBootstrapLogger();

		try
		{
			CreateHostBuilder(args)
				.Build()
				.Run();
		}
		catch (Exception exception)
		{
			logger.LogCritical(exception, "Web host terminated unexpectedly");
			throw;
		}
	}

	/// <summary>
	/// Creates and configures a new <see cref="IHostBuilder"/>.
	/// Used by WebApplicationFactory for in-memory integration tests and EF Core tools."/>.
	/// </summary>
	/// <param name="args">The command line arguments from which to parse configuration.</param>
	/// <returns>A configured web host builder.</returns>
	/// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host#set-up-a-host"/>
	[PublicAPI]
	public static IHostBuilder CreateHostBuilder(string[] args) => Host
		.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(builder =>
		{
			builder.UseStartup<Startup>();
			builder.ConfigureKestrel((context, kestrelOptions) =>
			{
				kestrelOptions.ConfigureHttpsDefaults(options => options.ConfigureCipherSuites(context));
			});

			if (GetContentRoot() is { } contentRoot)
			{
				builder.UseContentRoot(contentRoot);
			}

			if (GetWebRoot() is { } webRoot)
			{
				builder.UseWebRoot(webRoot);
			}
		});

	private static string? GetContentRoot() => Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT");

	private static string? GetWebRoot() => Environment.GetEnvironmentVariable("ASPNETCORE_WEBROOT");

	private static ILogger CreateBootstrapLogger()
	{
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddConsole());

		return services
			.BuildServiceProvider()
			.GetRequiredService<ILoggerFactory>()
			.CreateLogger(typeof(Program).FullName ?? nameof(Program));
	}
}

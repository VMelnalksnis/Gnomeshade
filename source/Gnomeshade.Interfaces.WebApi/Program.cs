// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Serilog;

namespace Gnomeshade.Interfaces.WebApi
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			try
			{
				Log.Information("Starting web host");

				var webHost = CreateWebHostBuilder(args).UseSerilog().Build();
				Log.Debug("Created web host");

				webHost.Run();
			}
			catch (Exception exception)
			{
				Log.Fatal(exception, "Web host terminated unexpectedly");
				throw;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		/// <summary>
		/// Creates and configures a new <see cref="IWebHostBuilder"/>.
		/// Used by WebApplicationFactory for in-memory integration tests and EF Core tools."/>.
		/// </summary>
		/// <param name="args">The command line arguments from which to parse configuration.</param>
		/// <returns>A configured web host builder.</returns>
		[PublicAPI]
		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder<Startup>(args);
		}
	}
}

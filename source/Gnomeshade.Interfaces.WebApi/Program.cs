// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;

using Gnomeshade.Interfaces.WebApi.Configuration;

using JetBrains.Annotations;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Serilog;

namespace Gnomeshade.Interfaces.WebApi;

public static class Program
{
	/// <summary>
	/// The application entry point.
	/// </summary>
	/// <param name="args">The command-line arguments of the application.</param>
	public static void Main(string[] args)
	{
		Serilog.Debugging.SelfLog.Enable(output => Debug.WriteLine(output));
		Log.Logger = SerilogWebHostConfiguration.CreateBoostrapLogger();

		try
		{
			CreateWebHostBuilder(args)
				.UseSerilog(SerilogWebHostConfiguration.Configure)
				.Build()
				.Run();
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
	/// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host#set-up-a-host"/>
	[PublicAPI]
	public static IWebHostBuilder CreateWebHostBuilder(string[] args)
	{
		return WebHost.CreateDefaultBuilder<Startup>(args);
	}
}

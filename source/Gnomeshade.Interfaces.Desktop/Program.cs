// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Reflection;

using Avalonia;
using Avalonia.Logging;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;

using Gnomeshade.Interfaces.WebApi.Models.Products;

using JetBrains.Annotations;

using Serilog;

namespace Gnomeshade.Interfaces.Desktop;

internal static class Program
{
	public static void Main(string[] args)
	{
		InitializeBootstrapLogger();

		try
		{
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "Avalonia App terminated unexpectedly");
			throw;
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}

	[UsedImplicitly]
	public static AppBuilder BuildAvaloniaApp()
	{
		if (Debugger.IsAttached)
		{
			typeof(Product).GetTypeInfo();
			typeof(Interaction).GetTypeInfo();
			typeof(EventTriggerBehavior).GetTypeInfo();
		}

		return
			AppBuilder
				.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace(LogEventLevel.Debug);
	}

	private static void InitializeBootstrapLogger()
	{
		Serilog.Debugging.SelfLog.Enable(output => Debug.WriteLine(output));
		Log.Logger = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo.Trace()
			.MinimumLevel.Debug()
			.CreateBootstrapLogger();
	}
}

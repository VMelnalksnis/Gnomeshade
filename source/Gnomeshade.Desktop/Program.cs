// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;

using Avalonia;
using Avalonia.Logging;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;

using Gnomeshade.Desktop.Authentication;
using Gnomeshade.WebApi.Models.Products;

using JetBrains.Annotations;

using Serilog;

namespace Gnomeshade.Desktop;

[SupportedOSPlatform("windows")]
internal static class Program
{
	public static void Main(string[] args)
	{
		SerilogConfiguration.InitializeBootstrapLogger();
		if (AnotherInstanceIsRunning())
		{
			SendArgumentsToRunningInstance(args);
			return;
		}

		try
		{
			using var handle = GetApplicationHandle();
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

	[SupportedOSPlatform("windows")]
	private static bool AnotherInstanceIsRunning()
	{
		Log.Debug("Checking if another instance is running");
		return EventWaitHandle.TryOpenExisting(WindowsProtocolHandler.Name, out _);
	}

	private static void SendArgumentsToRunningInstance(IEnumerable<string> args)
	{
		Log.Debug("Sending arguments to an already running instance");
		using var pipeClient = new NamedPipeClientStream(".", WindowsProtocolHandler.Name);
		pipeClient.Connect((int)TimeSpan.FromSeconds(5).TotalMilliseconds);

		using var streamWriter = new StreamWriter(pipeClient);
		streamWriter.Write(args.First());
		streamWriter.Flush();
	}

	[SupportedOSPlatform("windows")]
	private static IDisposable GetApplicationHandle()
	{
		Log.Debug("Getting an application handle");
		return new EventWaitHandle(false, EventResetMode.AutoReset, WindowsProtocolHandler.Name);
	}
}

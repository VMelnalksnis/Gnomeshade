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
using System.Threading.Tasks;

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
		Log.Debug("Working directory: {WorkingDirectory}", Environment.CurrentDirectory);

		if (AnotherInstanceIsRunning())
		{
			SendArgumentsToRunningInstance(args);
			Log.CloseAndFlush();
			return;
		}

		try
		{
			using var handle = GetApplicationHandle();

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

			BuildAvaloniaApp(args).StartWithClassicDesktopLifetime(args);
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "Avalonia App terminated unexpectedly");
		}
		finally
		{
			TaskScheduler.UnobservedTaskException -= TaskSchedulerOnUnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;

			Log.CloseAndFlush();
		}
	}

	[UsedImplicitly]
	public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp([]);

	private static AppBuilder BuildAvaloniaApp(string[] args)
	{
		if (Debugger.IsAttached)
		{
			typeof(Product).GetTypeInfo();
			typeof(Interaction).GetTypeInfo();
			typeof(EventTriggerBehavior).GetTypeInfo();
		}

		return
			AppBuilder
				.Configure<App>(() => new(args))
				.UsePlatformDetect()
				.LogToTrace(LogEventLevel.Debug);
	}

	private static bool AnotherInstanceIsRunning()
	{
		Log.Debug("Checking if another instance is running");
		return Mutex.TryOpenExisting(GnomeshadeProtocolHandler.Name, out _);
	}

	private static void SendArgumentsToRunningInstance(IEnumerable<string> args)
	{
		Log.Debug("Sending arguments to an already running instance");
		using var pipeClient = new NamedPipeClientStream(".", GnomeshadeProtocolHandler.Name);
		pipeClient.Connect((int)TimeSpan.FromSeconds(5).TotalMilliseconds);

		using var streamWriter = new StreamWriter(pipeClient);
		streamWriter.Write(args.First());
		streamWriter.Flush();
	}

	private static Mutex GetApplicationHandle()
	{
		Log.Debug("Getting an application handle");
		return new(false, GnomeshadeProtocolHandler.Name, out _);
	}

	private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		Log.Error(e.Exception, "Unobserved task exception");
	}

	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception exception)
		{
			Log.Error(exception, "Unobserved task exception");
		}
		else
		{
			Log.Error("Unobserved task exception, exception object is {TypeName}", e.ExceptionObject.GetType().FullName);
		}
	}
}

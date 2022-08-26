// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;

using NodaTime;

using Serilog;

namespace Gnomeshade.Desktop.Tests.Integration;

[SetUpFixture]
public static class AvaloniaSetup
{
	[OneTimeTearDown]
	public static async Task Stop()
	{
		var lifetime = await GetApp();
		await Invoke(() => lifetime.MainWindow.Close());
	}

	[OneTimeSetUp]
	public static async Task BuildAvaloniaApp()
	{
		var thread = new Thread(() =>
		{
			Serilog.Debugging.SelfLog.Enable(output => Debug.WriteLine(output));
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.NUnitOutput()
				.MinimumLevel.Debug()
				.CreateBootstrapLogger();

			var appBuilder = Program.BuildAvaloniaApp().UseHeadless();
			appBuilder.StartWithClassicDesktopLifetime(Array.Empty<string>());
		});

		thread.Start();

		// Need a better way to wait for app to be initialized, otherwise Dispatcher causes deadlock
		await Task.Delay(TimeSpan.FromSeconds(10));
	}

	internal static Task<IClassicDesktopStyleApplicationLifetime> GetApp() => Invoke(() =>
		(IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!);

	internal static Task Invoke(Action action) => Dispatcher.UIThread.InvokeAsync(action);

	internal static Task<T> Invoke<T>(Func<T> func) => Dispatcher.UIThread.InvokeAsync(func);

	internal static async Task PressKey(IInputRoot inputRoot, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
	{
		await KeyDown(inputRoot, key, modifiers);
		await Task.Delay(100);
		await KeyUp(inputRoot, key, modifiers);
	}

	internal static Task KeyDown(IInputRoot inputRoot, Key key, RawInputModifiers modifiers = RawInputModifiers.None) =>
		Invoke(() => KeyboardDevice.Instance!.ProcessRawEvent(
			new RawKeyEventArgs(
				KeyboardDevice.Instance,
				(ulong)SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks(),
				inputRoot,
				RawKeyEventType.KeyDown,
				key,
				modifiers)));

	internal static Task KeyUp(IInputRoot inputRoot, Key key, RawInputModifiers modifiers = RawInputModifiers.None) =>
		Invoke(() => KeyboardDevice.Instance!.ProcessRawEvent(
			new RawKeyEventArgs(
				KeyboardDevice.Instance,
				(ulong)SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks(),
				inputRoot,
				RawKeyEventType.KeyUp,
				key,
				modifiers)));
}

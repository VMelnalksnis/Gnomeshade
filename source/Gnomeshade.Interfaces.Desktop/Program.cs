﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia;
using Avalonia.Logging;

using Gnomeshade.Interfaces.Desktop.ViewModels.Design;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Desktop;

internal static class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	public static void Main(string[] args)
	{
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	[UsedImplicitly]
	public static AppBuilder BuildAvaloniaApp()
	{
		DesignTimeData.ForceAssembliesToLoad();

		return
			AppBuilder
				.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace(LogEventLevel.Debug);
	}
}

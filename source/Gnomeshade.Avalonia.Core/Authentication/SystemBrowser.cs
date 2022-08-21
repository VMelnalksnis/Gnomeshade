// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <inheritdoc />
public sealed class SystemBrowser : Browser
{
	/// <summary>Initializes a new instance of the <see cref="SystemBrowser"/> class.</summary>
	/// <param name="timeout">The time to wait until user completes signin.</param>
	public SystemBrowser(TimeSpan timeout)
		: base(timeout)
	{
	}

	internal static void OpenBrowser(string url)
	{
		// hack because of this: https://github.com/dotnet/corefx/issues/10361
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			url = url.Replace("&", "^&");
			Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			Process.Start("xdg-open", url);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			Process.Start("open", url);
		}
	}

	/// <inheritdoc />
	protected override void StartUserSignin(string startUrl) => OpenBrowser(startUrl);
}

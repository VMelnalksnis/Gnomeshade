// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <inheritdoc />
public sealed class SystemBrowser : Browser
{
	/// <summary>Initializes a new instance of the <see cref="SystemBrowser"/> class.</summary>
	/// <param name="gnomeshadeProtocolHandler">Handler for gnomeshade protocol requests.</param>
	/// <param name="timeout">The time to wait until user completes signin.</param>
	public SystemBrowser(IGnomeshadeProtocolHandler gnomeshadeProtocolHandler, TimeSpan timeout)
		: base(gnomeshadeProtocolHandler, timeout)
	{
	}

	internal static void OpenBrowser(string url)
	{
		// hack because of this: https://github.com/dotnet/corefx/issues/10361
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Process.Start(new ProcessStartInfo(url) { CreateNoWindow = true, UseShellExecute = true });
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
	protected override Task StartUserSignin(string startUrl, CancellationToken cancellationToken = default)
	{
		OpenBrowser(startUrl);
		return Task.CompletedTask;
	}
}

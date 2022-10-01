// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using IdentityModel.OidcClient.Browser;

using static IdentityModel.OidcClient.Browser.BrowserResultType;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <inheritdoc />
public abstract class Browser : IBrowser
{
	private readonly IGnomeshadeProtocolHandler _gnomeshadeProtocolHandler;
	private readonly TimeSpan _timeout;

	/// <summary>Initializes a new instance of the <see cref="Browser"/> class.</summary>
	/// <param name="gnomeshadeProtocolHandler">Handler for gnomeshade protocol requests.</param>
	/// <param name="timeout">The time to wait until user completes signin.</param>
	protected Browser(IGnomeshadeProtocolHandler gnomeshadeProtocolHandler, TimeSpan timeout)
	{
		_gnomeshadeProtocolHandler = gnomeshadeProtocolHandler;
		_timeout = timeout;
	}

	/// <inheritdoc />
	public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
	{
		var browserTask = StartUserSignin(options.StartUrl, cancellationToken);

		try
		{
			var task = _gnomeshadeProtocolHandler.GetRequestContent(cancellationToken);

			var timeoutTask = Task.Delay(_timeout, cancellationToken);
			await Task.WhenAny(timeoutTask, task);
			if (timeoutTask.IsCompleted)
			{
				return new() { ResultType = BrowserResultType.Timeout, Error = "Timeout" };
			}

			var result = task.Result;
			await browserTask;

			return string.IsNullOrWhiteSpace(result)
				? new() { ResultType = UnknownError, Error = "Empty response." }
				: new BrowserResult { Response = result, ResultType = Success };
		}
		catch (TaskCanceledException ex)
		{
			return new() { ResultType = BrowserResultType.Timeout, Error = ex.Message };
		}
		catch (Exception ex)
		{
			return new() { ResultType = UnknownError, Error = ex.Message };
		}
	}

	/// <summary>Starts the user sign-in process.</summary>
	/// <param name="startUrl">The URL for starting the sign-in process.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A <see cref="Task"/> representing the user sign-in process.</returns>
	protected abstract Task StartUserSignin(string startUrl, CancellationToken cancellationToken = default);
}

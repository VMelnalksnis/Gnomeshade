// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using IdentityModel.OidcClient.Browser;

using static System.Net.HttpStatusCode;
using static System.StringComparison;

using static IdentityModel.OidcClient.Browser.BrowserResultType;

namespace Gnomeshade.Interfaces.Avalonia.Core.Authentication;

/// <inheritdoc />
public sealed class SystemBrowser : IBrowser
{
	private const string _formUrlEncoded = "application/x-www-form-urlencoded";

	private readonly string _listenerPrefix;

	/// <summary>Initializes a new instance of the <see cref="SystemBrowser"/> class.</summary>
	/// <param name="listenerPrefix">The uri on which to listen for authorization redirect. <see cref="HttpListener.Prefixes"/>.</param>
	public SystemBrowser(string listenerPrefix)
	{
		_listenerPrefix = listenerPrefix;
	}

	/// <inheritdoc />
	public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
	{
		using var httpListener = new HttpListener();
		httpListener.Prefixes.Add(_listenerPrefix);
		httpListener.Start();

		OpenBrowser(options.StartUrl);

		try
		{
			// todo need to add timeout for waiting for redirect
			var httpContext = await httpListener.GetContextAsync().ConfigureAwait(false);
			var result = await HandeRequest(httpContext).ConfigureAwait(false);

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
		finally
		{
			httpListener.Stop();
		}
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

	private static async Task<string?> HandeRequest(HttpListenerContext context)
	{
		switch (context.Request.HttpMethod)
		{
			case "GET":
				await SuccessfulResponse(context).ConfigureAwait(false);
				return context.Request.Url?.Query;

			case "POST" when !context.Request.ContentType?.Equals(_formUrlEncoded, OrdinalIgnoreCase) ?? true:
				context.Response.StatusCode = (int)UnsupportedMediaType;
				return null;

			case "POST":
			{
				await SuccessfulResponse(context).ConfigureAwait(false);
				using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
				return await reader.ReadToEndAsync().ConfigureAwait(false);
			}

			default:
				context.Response.StatusCode = (int)MethodNotAllowed;
				return null;
		}
	}

	private static async Task SuccessfulResponse(HttpListenerContext context)
	{
		context.Response.StatusCode = (int)OK;
		context.Response.ContentType = "text/html";
		var streamWriter = new StreamWriter(context.Response.OutputStream);
		await using (streamWriter.ConfigureAwait(false))
		{
			await streamWriter.WriteAsync("<h1>You can now return to the application.</h1>").ConfigureAwait(false);
		}
	}
}

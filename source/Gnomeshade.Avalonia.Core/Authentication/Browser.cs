// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

using IdentityModel.OidcClient.Browser;

using static System.Net.HttpStatusCode;
using static System.StringComparison;

using static IdentityModel.OidcClient.Browser.BrowserResultType;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <inheritdoc />
public abstract class Browser : IBrowser
{
	private const string _formUrlEncoded = "application/x-www-form-urlencoded";

	private readonly TimeSpan _timeout;

	/// <summary>Initializes a new instance of the <see cref="Browser"/> class.</summary>
	/// <param name="timeout">The time to wait until user completes signin.</param>
	protected Browser(TimeSpan timeout)
	{
		_timeout = timeout;
	}

	/// <inheritdoc />
	public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
	{
		using var httpListener = new HttpListener();
		httpListener.Prefixes.Add(options.EndUrl);
		httpListener.Start();

		try
		{
			string? result = null;
			var getContext = httpListener.BeginGetContext(asyncResult => HandleRequest(asyncResult, ref result), httpListener);

			StartUserSignin(options.StartUrl);

			var receivedRequest = getContext.AsyncWaitHandle.WaitOne(_timeout);
			if (!receivedRequest)
			{
				throw new TaskCanceledException("Timed out while waiting for redirect after sign-in");
			}

			return Task.FromResult(string.IsNullOrWhiteSpace(result)
				? new() { ResultType = UnknownError, Error = "Empty response." }
				: new BrowserResult { Response = result, ResultType = Success });
		}
		catch (TaskCanceledException ex)
		{
			return Task.FromResult<BrowserResult>(new() { ResultType = BrowserResultType.Timeout, Error = ex.Message });
		}
		catch (Exception ex)
		{
			return Task.FromResult<BrowserResult>(new() { ResultType = UnknownError, Error = ex.Message });
		}
		finally
		{
			httpListener.Stop();
		}
	}

	/// <summary>Starts the user sign-in process.</summary>
	/// <param name="startUrl">The URL for starting the sign-in process.</param>
	protected abstract void StartUserSignin(string startUrl);

	// ref Used to get value out of callback
	// ReSharper disable once RedundantAssignment
	private static void HandleRequest(IAsyncResult asyncResult, ref string? result)
	{
		var httpListener = (HttpListener)asyncResult.AsyncState!;
		var context = httpListener.EndGetContext(asyncResult);
		result = HandleRequest(context);
		context.Response.OutputStream.Close();
	}

	private static string? HandleRequest(HttpListenerContext context) => context.Request.HttpMethod switch
	{
		"GET" => SuccessfulResponse(context, () => context.Request.Url?.Query),

		"POST" when !context.Request.ContentType?.Equals(_formUrlEncoded, OrdinalIgnoreCase) ?? true =>
			SetStatusCode(context, UnsupportedMediaType),

		"POST" => SuccessfulResponse(context, () =>
		{
			using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
			return reader.ReadToEnd();
		}),

		_ => SetStatusCode(context, MethodNotAllowed),
	};

	private static string? SuccessfulResponse(HttpListenerContext context, Func<string?> resultFunc)
	{
		context.Response.StatusCode = (int)OK;
		context.Response.ContentType = "text/html";

		const string resourceName = "SuccessfulResponsePage.html";
		var contentStream = Assembly
			.GetExecutingAssembly()
			.GetManifestResourceStream(typeof(SystemBrowser), resourceName);

		if (contentStream is null)
		{
			throw new MissingManifestResourceException(resourceName);
		}

		using (contentStream)
		{
			contentStream.CopyTo(context.Response.OutputStream);
		}

		return resultFunc();
	}

	private static string? SetStatusCode(HttpListenerContext context, HttpStatusCode statusCode)
	{
		context.Response.StatusCode = (int)statusCode;
		return null;
	}
}

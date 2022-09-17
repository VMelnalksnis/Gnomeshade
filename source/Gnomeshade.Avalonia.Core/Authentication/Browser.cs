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
	public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
	{
		using var httpListener = new HttpListener();
		httpListener.Prefixes.Add(options.EndUrl);
		httpListener.Start();

		var browserTask = StartUserSignin(options.StartUrl, cancellationToken);

		try
		{
			var timeoutTask = Task.Delay(_timeout, cancellationToken);
			var contextTask = httpListener.GetContextAsync();
			await Task.WhenAny(timeoutTask, contextTask);
			if (timeoutTask.IsCompleted)
			{
				return new() { ResultType = BrowserResultType.Timeout, Error = "Timeout" };
			}

			var httpContext = contextTask.Result;
			var result = await HandeRequest(httpContext);
			httpContext.Response.Close();

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
		finally
		{
			httpListener.Stop();
		}
	}

	/// <summary>Starts the user sign-in process.</summary>
	/// <param name="startUrl">The URL for starting the sign-in process.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A <see cref="Task"/> representing the user sign-in process.</returns>
	protected abstract Task StartUserSignin(string startUrl, CancellationToken cancellationToken = default);

	private static Task<string?> HandeRequest(HttpListenerContext context) => context.Request.HttpMethod switch
	{
		"GET" => SuccessfulResponse(context, () => Task.FromResult(context.Request.Url?.Query)),

		"POST" when !context.Request.ContentType?.Equals(_formUrlEncoded, OrdinalIgnoreCase) ?? true =>
			SetStatusCode(context, UnsupportedMediaType),

		"POST" => SuccessfulResponse(context, () =>
		{
			using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
			return reader.ReadToEndAsync()!;
		}),

		_ => SetStatusCode(context, MethodNotAllowed),
	};

	private static async Task<string?> SuccessfulResponse(HttpListenerContext context, Func<Task<string?>> resultFunc)
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

		await using (contentStream)
		{
			await contentStream.CopyToAsync(context.Response.OutputStream);
		}

		return await resultFunc();
	}

	private static Task<string?> SetStatusCode(HttpListenerContext context, HttpStatusCode statusCode)
	{
		context.Response.StatusCode = (int)statusCode;
		return Task.FromResult(default(string));
	}
}

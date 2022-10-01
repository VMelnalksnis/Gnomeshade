// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;

using static System.Net.HttpStatusCode;

namespace Gnomeshade.WebApi.Tests.Integration;

internal sealed class MockProtocolHandler : IGnomeshadeProtocolHandler
{
	private readonly string _uriPrefix;

	public MockProtocolHandler(string uriPrefix)
	{
		_uriPrefix = uriPrefix;
	}

	public async Task<string> GetRequestContent(CancellationToken cancellationToken = default)
	{
		using var httpListener = new HttpListener();
		httpListener.Prefixes.Add(_uriPrefix);
		httpListener.Start();

		var context = await httpListener.GetContextAsync();
		var result = HandeRequest(context);
		context.Response.Close();

		return result;
	}

	private static string HandeRequest(HttpListenerContext context) => context.Request.HttpMethod switch
	{
		"GET" => SetStatusCode(context, NoContent, context.Request.Url?.Query),
		_ => SetStatusCode(context, MethodNotAllowed),
	};

	private static string SetStatusCode(HttpListenerContext context, HttpStatusCode statusCode, string? result = default)
	{
		context.Response.StatusCode = (int)statusCode;
		return result ?? string.Empty;
	}
}

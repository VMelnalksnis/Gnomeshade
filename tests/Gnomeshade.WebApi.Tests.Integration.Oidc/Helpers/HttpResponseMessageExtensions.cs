// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Helpers;

internal static class HttpResponseMessageExtensions
{
	internal static Uri GetRedirect(this HttpResponseMessage response)
	{
		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location.Should().NotBeNull();
		return response.Headers.Location!;
	}

	internal static async Task EnsureSuccessAsync(this HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var content = await response.Content.ReadAsStringAsync();
		throw new HttpRequestException(content, null, response.StatusCode);
	}

	internal static string GetCookie(this HttpResponseMessage response) => response
		.Headers
		.GetValues("Set-Cookie")
		.Select(value => value.Split('=', StringSplitOptions.TrimEntries))
		.Aggregate(string.Empty, (current, split) => current + $"{split[0]}={split[1][..split[1].IndexOf(';')]}; ");
}

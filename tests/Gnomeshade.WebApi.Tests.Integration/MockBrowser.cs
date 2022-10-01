// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;

using HtmlAgilityPack;

namespace Gnomeshade.WebApi.Tests.Integration;

internal sealed class MockBrowser : Browser
{
	private readonly HttpClient _httpClient;

	public MockBrowser(IGnomeshadeProtocolHandler gnomeshadeProtocolHandler, HttpClient httpClient)
		: base(gnomeshadeProtocolHandler, TimeSpan.FromMinutes(1))
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc/>
	protected override async Task StartUserSignin(string startUrl, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.GetAsync(startUrl, cancellationToken);
		var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
		var htmlDocument = new HtmlDocument();
		htmlDocument.Load(responseContent);
		var loginUrl = htmlDocument
			.DocumentNode
			.SelectSingleNode("//form")
			.Attributes["action"]
			.Value
			.Replace("&amp;", "&");

		var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
		{
			new("username", "john.doe"),
			new("password", "test"),
			new("credentialId", string.Empty),
		});

		var cookie = response.Headers
			.GetValues("Set-Cookie")
			.Select(value => value.Split('=', StringSplitOptions.TrimEntries))
			.Aggregate(
				string.Empty,
				(current, split) => current + $"{split[0]}={split[1][..split[1].IndexOf(';')]}; ");

		_httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

		var loginResponse = await _httpClient.PostAsync(loginUrl, formContent, cancellationToken);
		loginResponse.EnsureSuccessStatusCode();
	}
}

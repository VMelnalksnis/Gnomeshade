// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gnomeshade.Avalonia.Core.Authentication;

using HtmlAgilityPack;

namespace Gnomeshade.WebApi.Tests.Integration;

internal sealed class MockBrowser : Browser
{
	private readonly HttpClient _httpClient;

	public MockBrowser(HttpClient httpClient, TimeSpan? timeout = null)
		: base(timeout ?? TimeSpan.FromMinutes(1))
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc/>
	protected override void StartUserSignin(string startUrl)
	{
		var response = _httpClient.Send(new(HttpMethod.Get, startUrl));
		var responseContent = response.Content.ReadAsStream();
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

		var loginResponse = _httpClient.Send(new(HttpMethod.Post, loginUrl) { Content = formContent });
		loginResponse.EnsureSuccessStatusCode();
	}
}

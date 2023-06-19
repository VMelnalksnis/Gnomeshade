// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;

using HtmlAgilityPack;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

internal sealed class MockBrowser : Browser
{
	private readonly HttpClient _httpClient;
	private readonly HttpClient _internalClient;
	private readonly string _username;
	private readonly string _password;

	public MockBrowser(
		IGnomeshadeProtocolHandler gnomeshadeProtocolHandler,
		HttpClient httpClient,
		HttpClient internalClient,
		string username,
		string password)
		: base(gnomeshadeProtocolHandler, TimeSpan.FromSeconds(10))
	{
		_httpClient = httpClient;
		_internalClient = internalClient;
		_username = username;
		_password = password;
	}

	internal async Task RegisterUser(string startUrl)
	{
		var registerResponse = await _internalClient.GetAsync(startUrl);

		var cookie = registerResponse.GetCookie();
		_internalClient.DefaultRequestHeaders.Add("Cookie", cookie);

		var registerContent = await registerResponse.Content.ReadAsStreamAsync();
		var htmlDocument = new HtmlDocument();
		htmlDocument.Load(registerContent);
		var registerFormData = htmlDocument.GetForm("""//*[@id="external-account"]""");

		var postResponse = await _internalClient.PostAsync(registerFormData.Action, registerFormData.Content);
		var location = postResponse.GetRedirect();

		var postCookie = postResponse.GetCookie();
		_internalClient.DefaultRequestHeaders.Add("Cookie", postCookie);

		var redirectResponse = await _httpClient.GetAsync(location);
		if (redirectResponse.StatusCode is not HttpStatusCode.OK)
		{
			throw new();
		}

		var redirectContent = await redirectResponse.Content.ReadAsStreamAsync();
		htmlDocument.Load(redirectContent);
		var keycloakFormData = htmlDocument.GetForm("//form");

		var keycloakResponse = await _internalClient.PostAsync(keycloakFormData.Action, keycloakFormData.Content);
		var keycloakLocation = keycloakResponse.GetRedirect();

		var keycloakCookie = keycloakResponse.GetCookie();
		_internalClient.DefaultRequestHeaders.Add("Cookie", keycloakCookie);

		var gnomeshadeFormResponse = await _internalClient.GetAsync(keycloakLocation);
		gnomeshadeFormResponse.EnsureSuccessStatusCode();

		var formCookie = gnomeshadeFormResponse.GetCookie();
		_internalClient.DefaultRequestHeaders.Add("Cookie", formCookie);

		var gnomeshadeFormContent = await gnomeshadeFormResponse.Content.ReadAsStreamAsync();
		htmlDocument.Load(gnomeshadeFormContent);

		var gnomeshadeFormData = htmlDocument.GetForm("//form");

		var finalRedirectResponse = await _internalClient.PostAsync(gnomeshadeFormData.Action, gnomeshadeFormData.Content);
		finalRedirectResponse.GetRedirect().OriginalString.Should().Be("/");
	}

	/// <inheritdoc/>
	protected override async Task StartUserSignin(string startUrl, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.GetAsync(startUrl, cancellationToken);
		if (response.StatusCode is HttpStatusCode.NoContent)
		{
			return;
		}

		await response.EnsureSuccessAsync();
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
			new("username", _username),
			new("password", _password),
			new("credentialId", string.Empty),
		});

		var cookie = response.GetCookie();
		_httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

		var loginResponse = await _httpClient.PostAsync(loginUrl, formContent, cancellationToken);
		var location = loginResponse.GetRedirect();
		loginResponse = await _httpClient.GetAsync(location, cancellationToken);

		if (loginResponse.StatusCode is not HttpStatusCode.NoContent)
		{
			var content = await loginResponse.Content.ReadAsStringAsync(cancellationToken);
			throw new ApplicationException($"Failed to log in to Keycloak{Environment.NewLine}{content}");
		}
	}
}

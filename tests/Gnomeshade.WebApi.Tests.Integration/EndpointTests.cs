// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

using HtmlAgilityPack;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class EndpointTests : WebserverTests
{
	private static readonly Uri _loginUri = new("/Identity/Account/Login", UriKind.Relative);

	public EndpointTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[TestCaseSource(nameof(Endpoints))]
	public async Task ShouldReturnOk(Uri requestUri)
	{
		using var apiClient = Fixture.CreateHttpClient();

		using var response = await apiClient.GetAsync(requestUri);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[TestCaseSource(nameof(AdminEndpoints))]
	public async Task ShouldReturnOkForAdmin(Uri requestUri)
	{
		using var apiClient = await GetAdminClient();

		using var response = await apiClient.GetAsync(requestUri);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[TestCaseSource(nameof(UserEndpoints))]
	public async Task ShouldReturnOkForUser(Uri requestUri)
	{
		using var apiClient = await GetUserClient();

		using var response = await apiClient.GetAsync(requestUri);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[TestCaseSource(nameof(UserRedirects))]
	public async Task ShouldRedirectForUser(Uri requestUri, Uri redirectUri)
	{
		using var apiClient = await GetUserClient();

		using var response = await apiClient.GetAsync(requestUri);

		using var scope = new AssertionScope();
		response.StatusCode.Should().Be(HttpStatusCode.Redirect);

		var location = response.Headers.Location;
		if (location?.IsAbsoluteUri ?? false)
		{
			location.LocalPath.Should().BeEquivalentTo(redirectUri.ToString());
		}
		else
		{
			location?.ToString().Should().BeEquivalentTo(redirectUri.ToString());
		}
	}

	[TestCaseSource(nameof(Redirects))]
	public async Task ShouldRedirect(Uri requestUri, Uri redirectUri)
	{
		using var apiClient = Fixture.CreateHttpClient();

		using var response = await apiClient.GetAsync(requestUri);

		using var scope = new AssertionScope();
		response.StatusCode.Should().Be(HttpStatusCode.Found);

		var location = response.Headers.Location;
		if (location?.IsAbsoluteUri ?? false)
		{
			location.LocalPath.Should().BeEquivalentTo(redirectUri.ToString());
			location.Query.Should().BeEquivalentTo($"?ReturnUrl={UrlEncoder.Default.Encode(requestUri.ToString())}");
		}
		else
		{
			location?.ToString().Should().BeEquivalentTo(redirectUri.ToString());
		}
	}

	private static IEnumerable Endpoints()
	{
		yield return new Uri("/", UriKind.Relative);
		yield return new Uri("/Identity/Account/Register", UriKind.Relative);
		yield return new Uri("/swagger/index.html", UriKind.Relative);
		yield return new Uri("/swagger/v1/swagger.json", UriKind.Relative);
		yield return new Uri("/Identity/Account/Register", UriKind.Relative);
		yield return new Uri("/Identity/Account/ResendEmailConfirmation", UriKind.Relative);
	}

	private static IEnumerable AdminEndpoints()
	{
		yield return new Uri("/Admin/Users", UriKind.Relative);
	}

	private static IEnumerable UserEndpoints()
	{
		yield return new Uri("/Identity/Account/Manage", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/ChangePassword", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/DeletePersonalData", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/Email", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/EnableAuthenticator", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/ExternalLogins", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/PersonalData", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/ResetAuthenticator", UriKind.Relative);
		yield return new Uri("/Identity/Account/Manage/TwoFactorAuthentication", UriKind.Relative);
		yield return new Uri("/Identity/Account/ForgotPasswordConfirmation", UriKind.Relative);
		yield return new Uri("/Identity/Account/AccessDenied", UriKind.Relative);
		yield return new Uri("/Identity/Account/Lockout", UriKind.Relative);
		yield return new Uri("/Identity/Account/Logout", UriKind.Relative);
	}

	private static IEnumerable UserRedirects()
	{
		yield return new TestCaseData(
			new Uri("/Admin/Users", UriKind.Relative),
			new Uri("/Identity/Account/AccessDenied", UriKind.Relative));

		yield return new TestCaseData(
			new Uri("/Identity/Account/Manage/Disable2fa", UriKind.Relative),
			new Uri("/Identity/Account/Manage/TwoFactorAuthentication", UriKind.Relative));

		yield return new TestCaseData(
			new Uri("/Identity/Account/Manage/GenerateRecoveryCodes", UriKind.Relative),
			new Uri("/Identity/Account/Manage/TwoFactorAuthentication", UriKind.Relative));

		yield return new TestCaseData(
			new Uri("/Identity/Account/Manage/SetPassword", UriKind.Relative),
			new Uri("/Identity/Account/Manage/ChangePassword", UriKind.Relative));

		yield return new TestCaseData(
			new Uri("/Identity/Account/Manage/ShowRecoveryCodes", UriKind.Relative),
			new Uri("/Identity/Account/Manage/TwoFactorAuthentication", UriKind.Relative));
	}

	private static IEnumerable Redirects()
	{
		yield return new TestCaseData(
			new Uri("/Identity/Account/ExternalLogin", UriKind.Relative),
			_loginUri);

		yield return new TestCaseData(
			new Uri("/Identity/Account/LoginWith2Fa", UriKind.Relative),
			_loginUri);

		yield return new TestCaseData(
			new Uri("/Identity/Account/LoginWithRecoveryCode", UriKind.Relative),
			_loginUri);

		yield return new TestCaseData(
			new Uri("/Identity/Account/Logout", UriKind.Relative),
			_loginUri);
	}

	private static string GetCookieValue(HttpResponseMessage response) => response
		.Headers
		.GetValues("Set-Cookie")
		.Select(value => value.Split('=', StringSplitOptions.TrimEntries))
		.Aggregate(
			string.Empty,
			(current, split) => current + $"{split[0]}={split[1][..split[1].IndexOf(';')]}; ");

	private async Task<HttpClient> GetAdminClient()
	{
		using var scope = Fixture.CreateScope();
		var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<AdminOptions>>().Value;
		return await GetAuthenticatedClient(options.Username, options.Password);
	}

	private async Task<HttpClient> GetUserClient()
	{
		var login = await Fixture.CreateApplicationUserAsync();
		return await GetAuthenticatedClient(login.Username, login.Password);
	}

	private async Task<HttpClient> GetAuthenticatedClient(string username, string password)
	{
		var httpClient = Fixture.CreateHttpClient();

		using var getResponse = await httpClient.GetAsync(_loginUri);
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		await using var responseContent = await getResponse.Content.ReadAsStreamAsync();
		var htmlDocument = new HtmlDocument();
		htmlDocument.Load(responseContent);

		var token = htmlDocument.DocumentNode
			.SelectSingleNode("//form[@id='account']/input[@name='__RequestVerificationToken']")
			.Attributes["value"]
			.Value;

		var formContent = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
		{
			new("Input.Username", username),
			new("Input.Password", password),
			new("__RequestVerificationToken", token),
			new("Input.RememberMe", "false"),
		});

		var antiForgeryCookies = GetCookieValue(getResponse);
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, _loginUri) { Content = formContent };
		requestMessage.Headers.Add("cookie", antiForgeryCookies);

		using var postResponse = await httpClient.SendAsync(requestMessage);
		postResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

		var authenticationCookie = GetCookieValue(postResponse);
		httpClient.DefaultRequestHeaders.Add("Cookie", authenticationCookie);
		return httpClient;
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	public sealed class FinanceClient : IFinanceClient
	{
		private static readonly string Authentication = typeof(AuthenticationController).GetControllerName();

		private static readonly string LoginUri = $"{Authentication}/{nameof(AuthenticationController.Login)}";
		private static readonly string InfoUri = $"{Authentication}/{nameof(AuthenticationController.Info)}";

		private readonly HttpClient _httpClient = new();

		public FinanceClient()
			: this(new Uri("https://localhost:44320/api/v1.0/"))
		{
		}

		public FinanceClient(Uri baseUri)
		{
			_httpClient.BaseAddress = baseUri;
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		/// <inheritdoc/>
		public async Task<UserModel> Info()
		{
			using var response = await _httpClient.GetAsync(InfoUri);
			response.EnsureSuccessStatusCode();

			return (await response.Content.ReadFromJsonAsync<UserModel>())!;
		}

		/// <inheritdoc/>
		public async Task<LoginResponse> Login(LoginModel login)
		{
			using var response = await _httpClient.PostAsJsonAsync(LoginUri, login);
			response.EnsureSuccessStatusCode();

			var loginResponse = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
			_httpClient.DefaultRequestHeaders.Authorization = GetAuthenticationHeader(loginResponse.Token);

			return loginResponse;
		}

		private static AuthenticationHeaderValue GetAuthenticationHeader(string token)
		{
			return new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
		}
	}
}

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	public sealed class FinanceClient : IFinanceClient
	{
		private static readonly string LoginUri =
			$"{typeof(AuthenticationController).GetControllerName()}/{nameof(AuthenticationController.Login)}";

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
		public async Task<LoginResponse> Login(LoginModel login)
		{
			var requestContent = JsonContent.Create(login);
			using var response = await _httpClient.PostAsync(LoginUri, requestContent);

			response.EnsureSuccessStatusCode();
			return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
		}
	}
}

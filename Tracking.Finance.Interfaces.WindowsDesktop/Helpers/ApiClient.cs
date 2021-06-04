using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WindowsDesktop.Models;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Helpers
{
	public sealed class ApiClient : IApiClient
	{
		private readonly HttpClient _apiClient;

		public ApiClient()
		{
			_apiClient = new HttpClient();
			_apiClient.DefaultRequestHeaders.Accept.Clear();
			_apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		/// <inheritdoc/>
		public async Task<AuthenticatedUser?> Authenticate(string username, string password)
		{
			var content = JsonContent.Create(new { username, password });
			using var response = await _apiClient.PostAsync("https://localhost:44320/api/v1.0/Authentication/Login", content);

			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<AuthenticatedUser>();
		}
	}
}

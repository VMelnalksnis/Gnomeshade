// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VMelnalksnis.OAuth2.Responses;

using static VMelnalksnis.OAuth2.FieldNames;

namespace VMelnalksnis.OAuth2;

public class OAuth2Client : IOAuth2Client
{
	private readonly HttpClient _httpClient;
	private readonly IOptionsMonitor<OAuth2ClientOptions> _clientOptionsMonitor;
	private readonly List<KeyValuePair<string, string>> _baseRequestValues = new();
	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

	public OAuth2Client(
		HttpClient httpClient,
		IOptionsMonitor<OAuth2ClientOptions> clientOptionsMonitor,
		ILogger<OAuth2Client> logger)
	{
		_httpClient = httpClient;
		_clientOptionsMonitor = clientOptionsMonitor;
		Logger = logger;

		UpdateConfiguration(_clientOptionsMonitor.CurrentValue);
		_clientOptionsMonitor.OnChange(UpdateConfiguration);
	}

	/// <summary>Gets the current value of client options.</summary>
	protected OAuth2ClientOptions Options => _clientOptionsMonitor.CurrentValue;

	protected ILogger<OAuth2Client> Logger { get; }

	/// <inheritdoc />
	/// <seealso cref="DeviceAuthorizationResponseExtensions.GetProcessStartInfoForUserApproval" />
	public async Task<DeviceAuthorizationResponse> StartDeviceFlowAsync(CancellationToken cancellationToken = default)
	{
		var content = new FormUrlEncodedContent(_baseRequestValues);
		var response = await _httpClient.PostAsync(Options.Device, content, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await ReadJsonResponse<DeviceAuthorizationResponse>(response, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<DeviceAuthorizationResponse> StartDeviceFlowAsync(string scope, CancellationToken cancellationToken = default)
	{
		var requestValues = _baseRequestValues.ToList();
		requestValues.Add(new(_scope, scope));
		var requestContent = new FormUrlEncodedContent(requestValues);
		var response = await _httpClient.PostAsync(Options.Device, requestContent, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await ReadJsonResponse<DeviceAuthorizationResponse>(response, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<TokenResponse> GetDeviceFlowResultAsync(
		DeviceAuthorizationResponse deviceAuthorizationResponse,
		CancellationToken cancellationToken = default)
	{
		var pollingInterval = TimeSpan.FromSeconds(deviceAuthorizationResponse.Interval);
		var endTime = DateTime.Now.AddSeconds(deviceAuthorizationResponse.ExpiresIn);

		var requestValues = _baseRequestValues.ToList();
		requestValues.Add(GrantTypes.DeviceCode);
		requestValues.Add(new(_deviceCode, deviceAuthorizationResponse.DeviceCode));
		var requestContent = new FormUrlEncodedContent(requestValues);

		Logger.LogDebug("Polling status with interval {PollingInterval} until {PollingEndTime}", pollingInterval, endTime);
		while (DateTime.Now < endTime)
		{
			Logger.LogDebug("Waiting for {PollingInternal} until next status check", pollingInterval);
			await Task.Delay(pollingInterval, cancellationToken);

			var response = await _httpClient.PostAsync(Options.Token, requestContent, cancellationToken);
			if (response.IsSuccessStatusCode)
			{
				Logger.LogDebug("Received token response");
				var tokenResponse = await ReadJsonResponse<TokenResponse>(response, cancellationToken);
				if (tokenResponse?.AccessToken is not null)
				{
					return tokenResponse;
				}

				if (tokenResponse is not null)
				{
					continue;
				}
			}

			var (error, description) = await ReadJsonResponse<TokenErrorResponse>(response, cancellationToken);
			Logger.LogDebug("Receiver error response {Error} with description {ErrorDescription}", error, description);
			switch (error)
			{
				case "authorization_pending":
					break;

				case "slow_down":
					pollingInterval += Options.PollingBackoff;
					break;

				default:
					throw new ApplicationException($"Authorization failed: {error} - {description}");
			}
		}

		throw new ApplicationException("Reached token expiration");
	}

	/// <inheritdoc />
	public async Task<TokenResponse> RefreshTokenAsync(
		TokenResponse tokenResponse,
		CancellationToken cancellationToken = default)
	{
		var requestValues = _baseRequestValues.ToList();
		requestValues.Add(GrantTypes.RefreshToken);
		requestValues.Add(new(_refreshToken, tokenResponse.RefreshToken));
		var requestContent = new FormUrlEncodedContent(requestValues);
		var response = await _httpClient.PostAsync(Options.Token, requestContent, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await ReadJsonResponse<TokenResponse>(response, cancellationToken);
	}

	/// <inheritdoc />
	public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		var requestValues = _baseRequestValues.ToList();
		requestValues.Add(new(_token, token));
		var requestContent = new FormUrlEncodedContent(requestValues);
		var response = await _httpClient.PostAsync(Options.Revoke, requestContent, cancellationToken); // todo
		response.EnsureSuccessStatusCode();
	}

	private Task<T?> ReadJsonResponse<T>(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
	{
		return responseMessage.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions, cancellationToken);
	}

	private void UpdateConfiguration(OAuth2ClientOptions options)
	{
		Logger.LogDebug("Updating configuration");

		_httpClient.BaseAddress = options.BaseAddress;
		_httpClient.DefaultRequestHeaders.Accept.Clear();
		_httpClient.DefaultRequestHeaders.Accept.Add(new(MediaTypeNames.Application.Json));

		_baseRequestValues.Clear();
		_baseRequestValues.Add(new(_clientId, options.ClientId));
		if (options.ClientSecret is not null)
		{
			_baseRequestValues.Add(new(_clientSecret, options.ClientSecret));
		}
	}
}

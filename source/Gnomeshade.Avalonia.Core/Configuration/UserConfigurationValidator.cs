// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Validates user configuration.</summary>
public sealed class UserConfigurationValidator
{
	private readonly ILogger<UserConfigurationValidator> _logger;
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of the <see cref="UserConfigurationValidator"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="httpClient">An HTTP client used to validate URIs.</param>
	public UserConfigurationValidator(ILogger<UserConfigurationValidator> logger, HttpClient httpClient)
	{
		_logger = logger;
		_httpClient = httpClient;
	}

	/// <summary>Checks whether the configuration is valid.</summary>
	/// <param name="userConfiguration">The configuration to validate.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns><see langword="true"/> if <paramref name="userConfiguration"/> is valid; otherwise <see langword="false"/>.</returns>
	public async Task<bool> IsValid(UserConfiguration userConfiguration, CancellationToken cancellationToken = default)
	{
		var baseAddress = userConfiguration.Gnomeshade?.BaseAddress;
		if (baseAddress is null)
		{
			return false;
		}

		var uriBuilder = new UriBuilder(baseAddress) { Path = "health" };
		try
		{
			var response = await _httpClient.SendAsync(new(HttpMethod.Get, uriBuilder.Uri), cancellationToken);
			if (response.StatusCode is not HttpStatusCode.OK)
			{
				return false;
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to check access to Gnomeshade API");
			return false;
		}

		var authority = userConfiguration.Oidc?.Authority;
		if (authority is null)
		{
			return false;
		}

		try
		{
			return (await _httpClient.SendAsync(new(HttpMethod.Get, authority), cancellationToken)).StatusCode is HttpStatusCode.OK;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to check access to OIDC provider");
			return false;
		}
	}
}

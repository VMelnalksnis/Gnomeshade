// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>View model for configuring options for authentication.</summary>
public sealed partial class AuthenticationConfigurationViewModel : ConfigurationViewModel
{
	private readonly ILogger<AuthenticationConfigurationViewModel> _logger;
	private readonly HttpClient _httpClient;

	private CancellationTokenSource? _cancellationTokenSource;

	/// <inheritdoc cref="OidcOptions.Authority"/>
	[Notify]
	[AlsoNotify(nameof(IsValid))]
	[PropertyAttribute("[System.ComponentModel.DataAnnotations.Required]")]
	private string? _authority;

	/// <inheritdoc cref="OidcOptions.ClientId"/>
	[Notify]
	[AlsoNotify(nameof(IsValid))]
	[PropertyAttribute("[System.ComponentModel.DataAnnotations.Required]")]
	private string? _clientId;

	/// <inheritdoc cref="OidcOptions.ClientSecret"/>
	[Notify]
	[AlsoNotify(nameof(IsValid))]
	private string? _clientSecret;

	/// <summary>Initializes a new instance of the <see cref="AuthenticationConfigurationViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="optionsMonitor">Options monitor of user preferences.</param>
	/// <param name="configurationWriter">Used to persist user configuration.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="httpClient">HTTP client URI validation.</param>
	public AuthenticationConfigurationViewModel(
		IActivityService activityService,
		IOptionsMonitor<UserConfiguration> optionsMonitor,
		UserConfigurationWriter configurationWriter,
		ILogger<AuthenticationConfigurationViewModel> logger,
		HttpClient httpClient)
		: base(activityService, optionsMonitor, configurationWriter)
	{
		_logger = logger;
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public override Task<bool> IsValid => IsValidAsync();

	/// <inheritdoc />
	internal override void UpdateConfiguration(UserConfiguration configuration)
	{
		if (Authority is null || ClientId is null)
		{
			throw new InvalidOperationException();
		}

		configuration.Oidc ??= new();
		configuration.Oidc.Authority = new(Authority);
		configuration.Oidc.ClientId = ClientId;
		configuration.Oidc.ClientSecret = ClientSecret;
	}

	private async Task<bool> IsValidAsync()
	{
		ErrorMessage = null;
		_cancellationTokenSource?.Cancel();

		if (Authority is null || ClientId is null)
		{
			return false;
		}

		_cancellationTokenSource = new();
		var cancellationToken = _cancellationTokenSource.Token;

		try
		{
			await Task.Delay(UserInputDelay, cancellationToken);
			using var activity = ActivityService.BeginActivity("Checking connectivity to authentication provider");

			using var response = await _httpClient.GetAsync(new Uri(Authority), cancellationToken);
			if (response.StatusCode is HttpStatusCode.OK)
			{
				return true;
			}

			ErrorMessage = $"Received status code {response.StatusCode:G} from authentication provider";
			return false;
		}
		catch (Exception exception) when (exception is not TaskCanceledException)
		{
			_logger.LogWarning(exception, "Failed to check authentication provider status");
			ErrorMessage = $"Failed to check the status of the authentication provider.{Environment.NewLine}{exception.Message}";
			return false;
		}
	}
}

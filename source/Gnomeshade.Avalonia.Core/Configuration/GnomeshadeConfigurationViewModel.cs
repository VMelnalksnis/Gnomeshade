// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>View model for configuring options for Gnomeshade API.</summary>
public sealed partial class GnomeshadeConfigurationViewModel : ConfigurationViewModel
{
	private readonly ILogger<GnomeshadeConfigurationViewModel> _logger;
	private readonly HttpClient _httpClient;

	private CancellationTokenSource? _cancellationTokenSource;

	/// <inheritdoc cref="GnomeshadeOptions.BaseAddress"/>
	[Notify]
	[AlsoNotify(nameof(IsValid))]
	[PropertyAttribute("[System.ComponentModel.DataAnnotations.Required]")]
	private string? _baseAddress;

	/// <summary>Initializes a new instance of the <see cref="GnomeshadeConfigurationViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="optionsMonitor">Options monitor of user preferences.</param>
	/// <param name="configurationWriter">Used to persist user configuration.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="httpClient">HTTP client URI validation.</param>
	public GnomeshadeConfigurationViewModel(
		IActivityService activityService,
		IOptionsMonitor<UserConfiguration> optionsMonitor,
		UserConfigurationWriter configurationWriter,
		ILogger<GnomeshadeConfigurationViewModel> logger,
		HttpClient httpClient)
		: base(activityService, optionsMonitor, configurationWriter)
	{
		_httpClient = httpClient;
		_logger = logger;

		_baseAddress = optionsMonitor.CurrentValue.Gnomeshade?.BaseAddress?.ToString();
	}

	/// <inheritdoc />
	public override Task<bool> IsValid => IsValidAsync();

	/// <inheritdoc />
	internal override void UpdateConfiguration(UserConfiguration configuration)
	{
		if (BaseAddress is null)
		{
			throw new InvalidOperationException();
		}

		configuration.Gnomeshade ??= new();
		configuration.Gnomeshade.BaseAddress = new(BaseAddress);
	}

	private async Task<bool> IsValidAsync()
	{
		ErrorMessage = null;
		_cancellationTokenSource?.Cancel();

		if (BaseAddress is null)
		{
			return false;
		}

		_cancellationTokenSource = new();
		var cancellationToken = _cancellationTokenSource.Token;

		try
		{
			var uriBuilder = new UriBuilder(BaseAddress) { Path = "/api/v1.0/health" };

			await Task.Delay(UserInputDelay, cancellationToken);
			using var activity = ActivityService.BeginActivity("Checking connectivity to API");

			using var response = await _httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
			if (response.StatusCode is not HttpStatusCode.OK)
			{
				ErrorMessage = $"Received status code {response.StatusCode:G} from API";
				return false;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			if (content is "Healthy")
			{
				return true;
			}

			ErrorMessage = $"Expected API to be healthy, actual status: {content}";
			return false;
		}
		catch (Exception exception) when (exception is not TaskCanceledException)
		{
			_logger.LogWarning(exception, "Failed to check Gnomeshade API status");
			ErrorMessage = $"Failed to check the status of the API.{Environment.NewLine}{exception.Message}";
			return false;
		}
	}
}

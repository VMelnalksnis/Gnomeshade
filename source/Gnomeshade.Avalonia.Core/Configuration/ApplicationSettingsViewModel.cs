// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Options;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Settings for this specific installation of the application.</summary>
public sealed class ApplicationSettingsViewModel : ViewModelBase
{
	private readonly UserConfigurationWriter _userConfigurationWriter;
	private readonly UserConfigurationValidator _validator;
	private readonly GnomeshadeOptions _gnomeshadeOptions;
	private readonly OidcOptions _oidcOptions;

	private CancellationTokenSource _cancellationTokenSource = new();
	private string _baseAddress;
	private string _oidcAuthority;
	private string _oidcClientId;
	private string? _oidcClientSecret;

	/// <summary>Initializes a new instance of the <see cref="ApplicationSettingsViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="userConfiguration">The current value of user configuration.</param>
	/// <param name="userConfigurationWriter">Used to persist user configuration.</param>
	/// <param name="validator">User configuration validator.</param>
	public ApplicationSettingsViewModel(
		IActivityService activityService,
		IOptions<UserConfiguration> userConfiguration,
		UserConfigurationWriter userConfigurationWriter,
		UserConfigurationValidator validator)
		: base(activityService)
	{
		_userConfigurationWriter = userConfigurationWriter;
		_validator = validator;

		_gnomeshadeOptions = userConfiguration.Value.Gnomeshade ?? new();
		_oidcOptions = userConfiguration.Value.Oidc ?? new();

		_baseAddress = _gnomeshadeOptions.BaseAddress?.ToString() ?? string.Empty;

		_oidcAuthority = _oidcOptions.Authority?.ToString() ?? string.Empty;
		_oidcClientId = _oidcOptions.ClientId;
		_oidcClientSecret = _oidcOptions.ClientSecret;
	}

	/// <summary>Raised when the user configuration has been updated.</summary>
	public event EventHandler? Updated;

	/// <inheritdoc cref="GnomeshadeOptions.BaseAddress"/>
	public string BaseAddress
	{
		get => _baseAddress;
		set
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new();
			if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
			{
				_gnomeshadeOptions.BaseAddress = new(value);
			}

			SetAndNotifyWithGuard(ref _baseAddress, value, nameof(BaseAddress), nameof(IsValid));
		}
	}

	/// <inheritdoc cref="OidcOptions.Authority"/>
	public string OidcAuthority
	{
		get => _oidcAuthority;
		set
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new();
			if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
			{
				_oidcOptions.Authority = new(value);
			}

			SetAndNotifyWithGuard(ref _oidcAuthority, value, nameof(OidcAuthority), nameof(IsValid));
		}
	}

	/// <inheritdoc cref="OidcOptions.ClientId"/>
	public string OidcClientId
	{
		get => _oidcClientId;
		set
		{
			_oidcOptions.ClientId = value;
			SetAndNotify(ref _oidcClientId, value);
		}
	}

	/// <inheritdoc cref="OidcOptions.ClientSecret"/>
	public string? OidcClientSecret
	{
		get => _oidcClientSecret;
		set
		{
			_oidcOptions.ClientSecret = value;
			SetAndNotify(ref _oidcClientSecret, value);
		}
	}

	/// <summary>Gets a task, the result of which indicates whether the current state of this view model represents valid user configuration.</summary>
	public Task<bool> IsValid => _validator.IsValid(Configuration, _cancellationTokenSource.Token);

	private UserConfiguration Configuration => new() { Gnomeshade = _gnomeshadeOptions, Oidc = _oidcOptions };

	/// <summary>Persists the changes made to the user configuration in this view model.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateConfiguration()
	{
		using var activity = BeginActivity();
		await _userConfigurationWriter.Write(Configuration);
		Updated?.Invoke(this, EventArgs.Empty);
	}
}

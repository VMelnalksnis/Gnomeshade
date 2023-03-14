// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Settings for this specific installation of the application.</summary>
public sealed partial class ApplicationSettingsViewModel : ViewModelBase
{
	private readonly IOptionsMonitor<UserConfiguration> _userConfiguration;
	private readonly UserConfigurationWriter _userConfigurationWriter;

	/// <summary>Gets or sets a value indicating whether authentication configuration is needed.</summary>
	[Notify]
	private bool _enableAuthentication;

	/// <summary>Initializes a new instance of the <see cref="ApplicationSettingsViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="userConfiguration">The current value of user configuration.</param>
	/// <param name="userConfigurationWriter">Used to persist user configuration.</param>
	/// <param name="gnomeshadeConfigurationViewModel">Gnomeshade API configuration viewmodel.</param>
	/// <param name="authenticationConfigurationViewModel">Authentication configuration viewmodel.</param>
	public ApplicationSettingsViewModel(
		IActivityService activityService,
		IOptionsMonitor<UserConfiguration> userConfiguration,
		UserConfigurationWriter userConfigurationWriter,
		GnomeshadeConfigurationViewModel gnomeshadeConfigurationViewModel,
		AuthenticationConfigurationViewModel authenticationConfigurationViewModel)
		: base(activityService)
	{
		_userConfiguration = userConfiguration;
		_userConfigurationWriter = userConfigurationWriter;

		Gnomeshade = gnomeshadeConfigurationViewModel;
		Authentication = authenticationConfigurationViewModel;
	}

	/// <summary>Raised when the user configuration has been updated.</summary>
	public event EventHandler? Updated;

	/// <summary>Gets the viewmodel for managing gnomeshade configuration.</summary>
	public GnomeshadeConfigurationViewModel Gnomeshade { get; }

	/// <summary>Gets the viewmodel for managing authentication configuration.</summary>
	public AuthenticationConfigurationViewModel Authentication { get; }

	/// <summary>Gets a task, the result of which indicates whether the current state of this view model represents valid user configuration.</summary>
	public Task<bool> IsValid => IsValidAsync();

	/// <summary>Persists the changes made to the user configuration in this view model.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateConfiguration()
	{
		using var activity = BeginActivity("Updating configuration");
		var configuration = _userConfiguration.CurrentValue;

		Gnomeshade.UpdateConfiguration(configuration);

		if (EnableAuthentication)
		{
			Authentication.UpdateConfiguration(configuration);
		}
		else
		{
			configuration.Oidc = null;
		}

		await _userConfigurationWriter.Write(configuration);
		Updated?.Invoke(this, EventArgs.Empty);
	}

	private async Task<bool> IsValidAsync()
	{
		var valid = await Gnomeshade.IsValid;
		if (!valid)
		{
			return false;
		}

		if (!EnableAuthentication)
		{
			return true;
		}

		return await Authentication.IsValid;
	}
}

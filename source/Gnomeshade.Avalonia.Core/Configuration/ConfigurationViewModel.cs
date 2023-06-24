// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Base class for view models that edit configuration.</summary>
public abstract class ConfigurationViewModel : ViewModelBase
{
	/// <summary>How long to wait after user input before validating.</summary>
	protected static readonly TimeSpan UserInputDelay = TimeSpan.FromMilliseconds(350);

	private readonly IOptionsMonitor<UserConfiguration> _optionsMonitor;
	private readonly UserConfigurationWriter _configurationWriter;

	/// <summary>Initializes a new instance of the <see cref="ConfigurationViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="optionsMonitor">Options monitor of user preferences.</param>
	/// <param name="configurationWriter">Used to persist user configuration.</param>
	protected ConfigurationViewModel(
		IActivityService activityService,
		IOptionsMonitor<UserConfiguration> optionsMonitor,
		UserConfigurationWriter configurationWriter)
		: base(activityService)
	{
		_optionsMonitor = optionsMonitor;
		_configurationWriter = configurationWriter;
	}

	/// <summary>Raised when the user configuration has been updated.</summary>
	public event EventHandler? Updated;

	/// <summary>Gets a value indicating whether the current configuration values are valid an can be saved.</summary>
	public abstract Task<bool> IsValid { get; }

	/// <summary>Persists the changes made by the user to the configuration.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateConfigurationAsync()
	{
		using var activity = BeginActivity("Updating configuration");

		var configuration = _optionsMonitor.CurrentValue;
		UpdateConfiguration(configuration);
		await _configurationWriter.Write(configuration);

		Updated?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Updates the configuration with the values supplied by the user.</summary>
	/// <param name="configuration">The configuration to update.</param>
	internal abstract void UpdateConfiguration(UserConfiguration configuration);
}

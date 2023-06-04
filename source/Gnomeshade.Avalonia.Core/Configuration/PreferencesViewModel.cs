// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>User preference editor.</summary>
public sealed partial class PreferencesViewModel : ViewModelBase
{
	private readonly IOptionsMonitor<UserConfiguration> _optionsMonitor;
	private readonly UserConfigurationWriter _userConfigurationWriter;
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly ILogger<PreferencesViewModel> _logger;

	/// <summary>Gets a collection of all available institutions.</summary>
	[Notify(Setter.Private)]
	private List<string> _institutions;

	/// <summary>Gets or sets the country for which to get <see cref="Institutions"/>.</summary>
	[Notify]
	private string? _nordigenCountry;

	/// <summary>Gets or sets the selected institution from <see cref="Institutions"/>.</summary>
	[Notify]
	private string? _selectedInstitutionId;

	/// <summary>Initializes a new instance of the <see cref="PreferencesViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="optionsMonitor">Options monitor of user preferences.</param>
	/// <param name="userConfigurationWriter">Used to persist user configuration.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public PreferencesViewModel(
		IActivityService activityService,
		IOptionsMonitor<UserConfiguration> optionsMonitor,
		UserConfigurationWriter userConfigurationWriter,
		IGnomeshadeClient gnomeshadeClient,
		ILogger<PreferencesViewModel> logger)
		: base(activityService)
	{
		_optionsMonitor = optionsMonitor;
		_userConfigurationWriter = userConfigurationWriter;
		_gnomeshadeClient = gnomeshadeClient;
		_logger = logger;

		_institutions = new();
		_nordigenCountry = _optionsMonitor.CurrentValue.Preferences?.NordigenCountry;
		_selectedInstitutionId = _optionsMonitor.CurrentValue.Preferences?.NoridgenInstitutionId;

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Saves the updates user preferences.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task Save()
	{
		var userConfiguration = _optionsMonitor.CurrentValue;

		var preferences = userConfiguration.Preferences ?? new();
		preferences.NordigenCountry = NordigenCountry;
		preferences.NoridgenInstitutionId = SelectedInstitutionId;

		await _userConfigurationWriter.Write(userConfiguration);
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(NordigenCountry) || NordigenCountry is not { } country)
		{
			return;
		}

		try
		{
			Institutions = await _gnomeshadeClient.GetInstitutionsAsync(country);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to get institutions");
		}
	}
}

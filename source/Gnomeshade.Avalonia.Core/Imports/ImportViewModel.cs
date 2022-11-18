﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Options;

namespace Gnomeshade.Avalonia.Core.Imports;

/// <summary>External data import view model.</summary>
public sealed class ImportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IOptionsMonitor<PreferencesOptions> _optionsMonitor;

	private string? _filePath;
	private List<string> _institutions;
	private string? _selectedInstitution;
	private string _country;

	/// <summary>Initializes a new instance of the <see cref="ImportViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="optionsMonitor">Options monitor of user preferences.</param>
	public ImportViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IOptionsMonitor<PreferencesOptions> optionsMonitor)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_optionsMonitor = optionsMonitor;
		_institutions = new();
		_country = _optionsMonitor.CurrentValue.NordigenCountry ?? "LV";
		_selectedInstitution = _optionsMonitor.CurrentValue.NoridgenInstitutionId;
	}

	/// <summary>Gets or sets the local path of the report file to import.</summary>
	public string? FilePath
	{
		get => _filePath;
		set => SetAndNotifyWithGuard(ref _filePath, value, nameof(FilePath), nameof(CanImport));
	}

	/// <summary>Gets or sets the country for which to get <see cref="Institutions"/>.</summary>
	public string Country
	{
		get => _country;
		set => SetAndNotify(ref _country, value);
	}

	/// <summary>Gets a collection of all available institutions.</summary>
	public List<string> Institutions
	{
		get => _institutions;
		private set => SetAndNotify(ref _institutions, value);
	}

	/// <summary>Gets or sets the selected institution from <see cref="Institutions"/>.</summary>
	public string? SelectedInstitution
	{
		get => _selectedInstitution;
		set => SetAndNotifyWithGuard(ref _selectedInstitution, value, nameof(SelectedInstitution), nameof(CanImport));
	}

	/// <summary>Gets a value indicating whether the information needed for <see cref="ImportAsync"/> is valid.</summary>
	public bool CanImport => !string.IsNullOrWhiteSpace(FilePath) || !string.IsNullOrWhiteSpace(SelectedInstitution);

	/// <summary>Imports the located at <see cref="FilePath"/>.</summary>
	/// <exception cref="InvalidOperationException"><see cref="FilePath"/> is null or whitespace.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task ImportAsync()
	{
		using var activity = BeginActivity("Importing transactions");
		if (FilePath is not null)
		{
			var file = new FileInfo(FilePath);
			await using var stream = file.OpenRead();
			await _gnomeshadeClient.Import(stream, file.Name);
		}
		else if (SelectedInstitution is not null)
		{
			var result = await _gnomeshadeClient.ImportAsync(SelectedInstitution);
			if (result is NewRequisition newRequisition)
			{
				SystemBrowser.OpenBrowser(newRequisition.RequisitionUri.ToString());
			}
		}

		throw new InvalidOperationException("Could not import");
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Institutions = await _gnomeshadeClient.GetInstitutionsAsync(_country);
		if (_optionsMonitor.CurrentValue.NoridgenInstitutionId is { } institutionId)
		{
			SelectedInstitution = Institutions.SingleOrDefault(id => id == institutionId) ?? Institutions.First();
		}
		else
		{
			SelectedInstitution = Institutions.First();
		}
	}
}

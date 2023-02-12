// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.WebApi.Client;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Overview and editing of all units.</summary>
public sealed partial class UnitViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets or sets the selected unit from <see cref="DataGridView"/>.</summary>
	[Notify]
	private UnitRow? _selectedUnit;

	private UnitCreationViewModel _unit;

	/// <summary>Gets a typed collection of all units.</summary>
	[Notify(Setter.Private)]
	private DataGridItemCollectionView<UnitRow> _units;

	private UnitViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IEnumerable<UnitRow> unitRows,
		UnitCreationViewModel unitCreationViewModel)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_unit = unitCreationViewModel;
		Unit.Upserted += OnUnitUpserted;

		_units = new(unitRows);

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets the grid view of all units.</summary>
	public DataGridCollectionView DataGridView => Units;

	/// <summary>Gets the current unit creation view model.</summary>
	public UnitCreationViewModel Unit
	{
		get => _unit;
		private set
		{
			Unit.Upserted -= OnUnitUpserted;
			SetAndNotify(ref _unit, value);
			Unit.Upserted += OnUnitUpserted;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="UnitViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="UnitViewModel"/> class.</returns>
	public static async Task<UnitViewModel> CreateAsync(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient)
	{
		var productRows = await gnomeshadeClient.GetUnitRowsAsync();
		var productCreation = await UnitCreationViewModel.CreateAsync(activityService, gnomeshadeClient);

		return new(activityService, gnomeshadeClient, productRows, productCreation);
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedUnit))
		{
			return;
		}

		Unit = await UnitCreationViewModel.CreateAsync(ActivityService, _gnomeshadeClient, SelectedUnit?.Id);
	}

	private async void OnUnitUpserted(object? sender, UpsertedEventArgs e)
	{
		var unitRowsTask = _gnomeshadeClient.GetUnitRowsAsync();
		var unitCreationTask = UnitCreationViewModel.CreateAsync(ActivityService, _gnomeshadeClient);

		var sortDescriptions = DataGridView.SortDescriptions;
		var units = (await unitRowsTask).ToList();
		Units = new(units);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Unit = await unitCreationTask;
	}
}

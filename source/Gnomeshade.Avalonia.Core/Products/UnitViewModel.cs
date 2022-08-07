// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Overview and editing of all units.</summary>
public sealed class UnitViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private UnitRow? _selectedUnit;
	private UnitCreationViewModel _unit;
	private DataGridItemCollectionView<UnitRow> _units;

	private UnitViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IEnumerable<UnitRow> unitRows,
		UnitCreationViewModel unitCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_unit = unitCreationViewModel;
		Unit.Upserted += OnUnitUpserted;

		_units = new(unitRows);

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets the grid view of all units.</summary>
	public DataGridCollectionView DataGridView => Units;

	/// <summary>Gets a typed collection of all units.</summary>
	public DataGridItemCollectionView<UnitRow> Units
	{
		get => _units;
		private set => SetAndNotifyWithGuard(ref _units, value, nameof(Units), nameof(DataGridView));
	}

	/// <summary>Gets or sets the selected unit from <see cref="DataGridView"/>.</summary>
	public UnitRow? SelectedUnit
	{
		get => _selectedUnit;
		set => SetAndNotify(ref _selectedUnit, value);
	}

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
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="UnitViewModel"/> class.</returns>
	public static async Task<UnitViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var productRows = await gnomeshadeClient.GetUnitRowsAsync().ConfigureAwait(false);
		var productCreation = await UnitCreationViewModel.CreateAsync(gnomeshadeClient).ConfigureAwait(false);

		return new(gnomeshadeClient, productRows, productCreation);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedUnit))
		{
			return;
		}

		Unit = Task.Run(() => UnitCreationViewModel.CreateAsync(_gnomeshadeClient, SelectedUnit?.Id)).Result;
	}

	private async void OnUnitUpserted(object? sender, UpsertedEventArgs e)
	{
		var unitRowsTask = _gnomeshadeClient.GetUnitRowsAsync().ConfigureAwait(false);
		var unitCreationTask = UnitCreationViewModel.CreateAsync(_gnomeshadeClient).ConfigureAwait(false);

		var sortDescriptions = DataGridView.SortDescriptions;
		var units = (await unitRowsTask).ToList();
		Units = new(units);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Unit = await unitCreationTask;
	}
}

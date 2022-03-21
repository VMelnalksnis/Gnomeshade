// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

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

	private void OnUnitUpserted(object? sender, UpsertedEventArgs e)
	{
		var unitRowsTask = Task.Run(() => _gnomeshadeClient.GetUnitRowsAsync());
		var unitCreationTask = Task.Run(() => UnitCreationViewModel.CreateAsync(_gnomeshadeClient));

		var sortDescriptions = DataGridView.SortDescriptions;
		Units = new(unitRowsTask.GetAwaiter().GetResult());
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Unit = unitCreationTask.GetAwaiter().GetResult();
	}
}

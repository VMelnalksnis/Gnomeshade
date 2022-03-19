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
	private readonly IProductClient _productClient;

	private UnitRow? _selectedUnit;
	private UnitCreationViewModel _unit;
	private DataGridItemCollectionView<UnitRow> _units;

	private UnitViewModel(
		IProductClient productClient,
		IEnumerable<UnitRow> unitRows,
		UnitCreationViewModel unitCreationViewModel)
	{
		_productClient = productClient;
		_unit = unitCreationViewModel;
		Unit.UnitCreated += OnUnitCreated;

		_units = new(unitRows);

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets the grid view of all units.</summary>
	public DataGridCollectionView DataGridView => Units;

	/// <summary>Gets a typed collection of all units.</summary>
	public DataGridItemCollectionView<UnitRow> Units
	{
		get => _units;
		private set => SetAndNotify(ref _units, value);
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
			Unit.UnitCreated -= OnUnitCreated;
			SetAndNotify(ref _unit, value);
			Unit.UnitCreated += OnUnitCreated;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="UnitViewModel"/> class.</summary>
	/// <param name="productClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="UnitViewModel"/> class.</returns>
	public static async Task<UnitViewModel> CreateAsync(IProductClient productClient)
	{
		var productRows = await productClient.GetUnitRowsAsync().ConfigureAwait(false);
		var productCreation = await UnitCreationViewModel.CreateAsync(productClient).ConfigureAwait(false);

		return new(productClient, productRows, productCreation);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedUnit))
		{
			return;
		}

		Unit = Task.Run(() => UnitCreationViewModel.CreateAsync(_productClient, SelectedUnit?.Id)).Result;
	}

	private void OnUnitCreated(object? sender, UnitCreatedEventArgs e)
	{
		var unitRowsTask = Task.Run(() => _productClient.GetUnitRowsAsync());
		var unitCreationTask = Task.Run(() => UnitCreationViewModel.CreateAsync(_productClient));

		var sortDescriptions = DataGridView.SortDescriptions;
		Units = new(unitRowsTask.GetAwaiter().GetResult());
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Unit = unitCreationTask.GetAwaiter().GetResult();
	}
}

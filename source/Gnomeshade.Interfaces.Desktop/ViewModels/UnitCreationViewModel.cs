// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>
/// Form for creating a single new unit.
/// </summary>
public sealed class UnitCreationViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private string? _name;
	private Unit? _parentUnit;
	private decimal? _multiplier;

	private UnitCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Unit> units)
	{
		_gnomeshadeClient = gnomeshadeClient;
		Units = units;

		UnitSelector = (_, item) => ((Unit)item).Name;
	}

	/// <summary>
	/// Raised when a new unit has been successfully created.
	/// </summary>
	public event EventHandler<UnitCreatedEventArgs>? UnitCreated;

	/// <summary>
	/// Gets or sets the name of the unit.
	/// </summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanCreate));
	}

	/// <summary>
	/// Gets or sets the unit on which this unit is based on.
	/// </summary>
	public Unit? ParentUnit
	{
		get => _parentUnit;
		set => SetAndNotifyWithGuard(ref _parentUnit, value, nameof(ParentUnit), nameof(CanCreate));
	}

	/// <summary>
	/// Gets a collection of all available units.
	/// </summary>
	public List<Unit> Units { get; }

	public AutoCompleteSelector<object> UnitSelector { get; }

	/// <summary>
	/// Gets or sets a multiplier for converting from this unit to <see cref="ParentUnit"/>.
	/// </summary>
	public decimal? Multiplier
	{
		get => _multiplier;
		set => SetAndNotifyWithGuard(ref _multiplier, value, nameof(Multiplier), nameof(ParentUnit));
	}

	/// <summary>
	/// Gets a value indicating whether or not a unit can be created from the currently specified values.
	/// </summary>
	public bool CanCreate =>
		!string.IsNullOrWhiteSpace(Name) &&
		((ParentUnit is null && Multiplier is null) || (ParentUnit is not null && Multiplier is not null));

	/// <summary>
	/// Asynchronously creates a new instance of the <see cref="UnitCreationViewModel"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <returns>A new instance of the <see cref="UnitCreationViewModel"/> class.</returns>
	public static async Task<UnitCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var units = await gnomeshadeClient.GetUnitsAsync();
		return new(gnomeshadeClient, units);
	}

	/// <summary>
	/// Creates a new unit form the specified values.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateUnitAsync()
	{
		var unit = new UnitCreationModel
		{
			Name = Name,
			ParentUnitId = ParentUnit?.Id,
			Multiplier = Multiplier,
		};

		var unitId = await _gnomeshadeClient.CreateUnitAsync(unit).ConfigureAwait(false);
		OnUnitCreated(unitId);
	}

	private void OnUnitCreated(Guid unitId)
	{
		UnitCreated?.Invoke(this, new(unitId));
	}
}

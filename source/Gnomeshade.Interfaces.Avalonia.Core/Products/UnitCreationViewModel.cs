// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Form for creating a single new unit.</summary>
public sealed class UnitCreationViewModel : ViewModelBase
{
	private readonly IProductClient _productClient;
	private readonly Unit? _existingUnit;

	private string? _name;
	private Unit? _parentUnit;
	private decimal? _multiplier;

	private UnitCreationViewModel(IProductClient productClient, List<Unit> units)
	{
		_productClient = productClient;
		Units = units;

		UnitSelector = (_, item) => ((Unit)item).Name;
	}

	private UnitCreationViewModel(IProductClient productClient, List<Unit> units, Unit existingUnit)
		: this(productClient, units)
	{
		_existingUnit = existingUnit;

		Name = existingUnit.Name;
		ParentUnit = existingUnit.ParentUnitId is null
			? null
			: units.Single(unit => unit.Id == existingUnit.ParentUnitId.Value);
		Multiplier = existingUnit.Multiplier;
	}

	/// <summary>Raised when a new unit has been successfully created.</summary>
	public event EventHandler<UnitCreatedEventArgs>? UnitCreated;

	/// <summary>Gets or sets the name of the unit.</summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanCreate));
	}

	/// <summary>Gets or sets the unit on which this unit is based on.</summary>
	public Unit? ParentUnit
	{
		get => _parentUnit;
		set => SetAndNotifyWithGuard(ref _parentUnit, value, nameof(ParentUnit), nameof(CanCreate));
	}

	/// <summary>Gets a collection of all available units.</summary>
	public List<Unit> Units { get; }

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector { get; }

	/// <summary>Gets or sets a multiplier for converting from this unit to <see cref="ParentUnit"/>.</summary>
	public decimal? Multiplier
	{
		get => _multiplier;
		set => SetAndNotifyWithGuard(ref _multiplier, value, nameof(Multiplier), nameof(ParentUnit));
	}

	/// <summary>Gets a value indicating whether or not a unit can be created from the currently specified values.</summary>
	public bool CanCreate =>
		!string.IsNullOrWhiteSpace(Name) &&
		((ParentUnit is null && Multiplier is null) || (ParentUnit is not null && Multiplier is not null));

	/// <summary>Asynchronously creates a new instance of the <see cref="UnitCreationViewModel"/> class.</summary>
	/// <param name="productClient">API client for getting finance data.</param>
	/// <param name="unitId">The id of the unit to edit.</param>
	/// <returns>A new instance of the <see cref="UnitCreationViewModel"/> class.</returns>
	public static async Task<UnitCreationViewModel> CreateAsync(IProductClient productClient, Guid? unitId = null)
	{
		var units = await productClient.GetUnitsAsync().ConfigureAwait(false);
		if (unitId is null)
		{
			return new(productClient, units);
		}

		var existingUnit = units.Single(unit => unit.Id == unitId.Value);
		return new(productClient, units, existingUnit);
	}

	/// <summary>Creates a new unit form the specified values.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateUnitAsync()
	{
		var unit = new UnitCreationModel
		{
			Name = Name,
			ParentUnitId = ParentUnit?.Id,
			Multiplier = Multiplier,
		};

		var id = _existingUnit?.Id ?? Guid.NewGuid();

		// todo put
		var unitId = await _productClient.CreateUnitAsync(unit).ConfigureAwait(false);
		OnUnitCreated(unitId);
	}

	private void OnUnitCreated(Guid unitId)
	{
		UnitCreated?.Invoke(this, new(unitId));
	}
}

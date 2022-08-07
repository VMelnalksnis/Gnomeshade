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

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Form for creating a single new unit.</summary>
public sealed class UnitCreationViewModel : UpsertionViewModel
{
	private static readonly string[] _canCreate = { nameof(CanSave) };

	private readonly Unit? _existingUnit;

	private string? _name;
	private Unit? _parentUnit;
	private decimal? _multiplier;

	private UnitCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Unit> units)
		: base(gnomeshadeClient)
	{
		Units = units;
	}

	private UnitCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Unit> units, Unit existingUnit)
		: this(gnomeshadeClient, units)
	{
		_existingUnit = existingUnit;

		Name = existingUnit.Name;
		ParentUnit = existingUnit.ParentUnitId is null
			? null
			: units.Single(unit => unit.Id == existingUnit.ParentUnitId.Value);
		Multiplier = existingUnit.Multiplier;
	}

	/// <summary>Gets or sets the name of the unit.</summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), _canCreate);
	}

	/// <summary>Gets or sets the unit on which this unit is based on.</summary>
	public Unit? ParentUnit
	{
		get => _parentUnit;
		set => SetAndNotifyWithGuard(ref _parentUnit, value, nameof(ParentUnit), _canCreate);
	}

	/// <summary>Gets a collection of all available units.</summary>
	public List<Unit> Units { get; }

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <summary>Gets or sets a multiplier for converting from this unit to <see cref="ParentUnit"/>.</summary>
	public decimal? Multiplier
	{
		get => _multiplier;
		set => SetAndNotifyWithGuard(ref _multiplier, value, nameof(Multiplier), _canCreate);
	}

	/// <inheritdoc />
	public override bool CanSave =>
		!string.IsNullOrWhiteSpace(Name) &&
		((ParentUnit is null && Multiplier is null) || (ParentUnit is not null && Multiplier is not null));

	/// <summary>Asynchronously creates a new instance of the <see cref="UnitCreationViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="unitId">The id of the unit to edit.</param>
	/// <returns>A new instance of the <see cref="UnitCreationViewModel"/> class.</returns>
	public static async Task<UnitCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid? unitId = null)
	{
		var units = await gnomeshadeClient.GetUnitsAsync().ConfigureAwait(false);
		if (unitId is null)
		{
			return new(gnomeshadeClient, units);
		}

		var existingUnit = units.Single(unit => unit.Id == unitId.Value);
		return new(gnomeshadeClient, units, existingUnit);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var unit = new UnitCreation
		{
			Name = Name,
			ParentUnitId = ParentUnit?.Id,
			Multiplier = Multiplier,
		};

		var id = _existingUnit?.Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutUnitAsync(id, unit).ConfigureAwait(false);
		return id;
	}
}

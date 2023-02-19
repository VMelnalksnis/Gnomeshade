// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Form for creating a single new unit.</summary>
public sealed partial class UnitUpsertionViewModel : UpsertionViewModel
{
	/// <summary>Gets or sets the name of the unit.</summary>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the unit on which this unit is based on.</summary>
	[Notify]
	private Unit? _parentUnit;

	/// <summary>Gets or sets a multiplier for converting from this unit to <see cref="ParentUnit"/>.</summary>
	[Notify]
	private decimal? _multiplier;

	/// <summary>Gets or sets the symbol of the unit.</summary>
	[Notify]
	private string? _symbol;

	/// <summary>Gets a collection of all available units.</summary>
	[Notify(Setter.Private)]
	private List<Unit> _units;

	/// <summary>Initializes a new instance of the <see cref="UnitUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the unit to edit.</param>
	public UnitUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		Id = id;

		_units = new();
	}

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <inheritdoc />
	public override bool CanSave =>
		!string.IsNullOrWhiteSpace(Name) &&
		((ParentUnit is null && Multiplier is null) || (ParentUnit is not null && Multiplier is not null));

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Units = await GnomeshadeClient.GetUnitsAsync();
		if (Id is not { } id)
		{
			return;
		}

		var existingUnit = Units.Single(unit => unit.Id == id);
		Name = existingUnit.Name;
		Multiplier = existingUnit.Multiplier;
		Symbol = existingUnit.Symbol;
		ParentUnit = existingUnit.ParentUnitId is null
			? null
			: Units.Single(unit => unit.Id == existingUnit.ParentUnitId.Value);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var unit = new UnitCreation
		{
			Name = Name,
			ParentUnitId = ParentUnit?.Id,
			Multiplier = Multiplier,
			Symbol = _symbol,
		};

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutUnitAsync(id, unit);
		return id;
	}
}

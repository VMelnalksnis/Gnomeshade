﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Products;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Overview of a single <see cref="Unit"/>.</summary>
public sealed partial class UnitRow : PropertyChangedBase
{
	/// <summary>Gets the name of the unit.</summary>
	[Notify(Setter.Internal)]
	private string _name;

	/// <summary>Initializes a new instance of the <see cref="UnitRow"/> class.</summary>
	/// <param name="unit">The unit this row represents.</param>
	/// <param name="units">A collection of units from which to select the parent unit.</param>
	public UnitRow(Unit unit, IEnumerable<Unit> units)
	{
		Id = unit.Id;
		_name = unit.Name;
		Symbol = unit.Symbol;
		ParentUnitName = unit.ParentUnitId is null
			? null
			: units.Single(u => u.Id == unit.ParentUnitId.Value).Name;
		Multiplier = unit.InverseMultiplier ? 1 / unit.Multiplier : unit.Multiplier;
	}

	/// <summary>Gets the id of the unit.</summary>
	public Guid Id { get; }

	/// <summary>Gets the symbol of the unit.</summary>
	public string? Symbol { get; }

	/// <summary>Gets the name of the parent unit.</summary>
	public string? ParentUnitName { get; }

	/// <summary>Gets the multiplier to convert a value in this unit to the parent unit.</summary>
	public decimal? Multiplier { get; }
}

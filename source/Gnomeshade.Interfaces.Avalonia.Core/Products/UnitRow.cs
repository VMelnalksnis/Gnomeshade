// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Overview of a single <see cref="Unit"/>.</summary>
public sealed class UnitRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="UnitRow"/> class.</summary>
	/// <param name="unit">The unit this row represents.</param>
	public UnitRow(Unit unit)
	{
		Id = unit.Id;
		Name = unit.Name;
	}

	/// <summary>Gets the id of the unit.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the unit.</summary>
	public string Name { get; }
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Products;

/// <summary>The information needed to create or update a unit.</summary>
[PublicAPI]
public sealed record UnitCreation : Creation
{
	/// <inheritdoc cref="Unit.Name"/>
	[Required]
	public string? Name { get; init; }

	/// <inheritdoc cref="Unit.ParentUnitId"/>
	[RequiredIfNotNull(nameof(Multiplier))]
	public Guid? ParentUnitId { get; init; }

	/// <inheritdoc cref="Unit.Multiplier"/>
	[RequiredIfNotNull(nameof(ParentUnitId))]
	public decimal? Multiplier { get; init; }
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Products;

/// <summary>The information needed to create or update a unit.</summary>
[PublicAPI]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code")]
public sealed record UnitCreation : Creation
{
	/// <inheritdoc cref="Unit.Name"/>
	[Required]
	public string? Name { get; set; }

	/// <inheritdoc cref="Unit.Symbol"/>
	public string? Symbol { get; set; }

	/// <inheritdoc cref="Unit.ParentUnitId"/>
	[RequiredIfNotNull(nameof(Multiplier))]
	public Guid? ParentUnitId { get; set; }

	/// <inheritdoc cref="Unit.Multiplier"/>
	[RequiredIfNotNull(nameof(ParentUnitId))]
	public decimal? Multiplier { get; set; }

	/// <inheritdoc cref="Unit.InverseMultiplier"/>
	public bool InverseMultiplier { get; set; }
}

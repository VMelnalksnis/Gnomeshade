// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Products;

/// <summary>A unit of measure.</summary>
[PublicAPI]
public sealed record Unit
{
	/// <summary>The id of the unit.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the unit was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the unit.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this unit.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the unit was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this unit.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The name of the unit.</summary>
	public string Name { get; set; } = null!;

	/// <summary>The symbol of the unit.</summary>
	public string? Symbol { get; set; }

	/// <summary>The id of the parent unit.</summary>
	public Guid? ParentUnitId { get; set; }

	/// <summary>The multiplier to convert a value in this unit to the parent unit.</summary>
	public decimal? Multiplier { get; set; }
}

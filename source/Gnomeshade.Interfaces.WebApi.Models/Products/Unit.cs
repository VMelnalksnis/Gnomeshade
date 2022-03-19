// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Products;

/// <summary>A unit of measure.</summary>
[PublicAPI]
public sealed record Unit
{
	/// <summary>The id of the unit.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the unit was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the owner of the unit.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this unit.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the unit was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this unit.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The name of the unit.</summary>
	public string Name { get; init; } = null!;

	/// <summary>The id of the parent unit.</summary>
	public Guid? ParentUnitId { get; init; }

	/// <summary>The multiplier to convert a value in this unit to the parent unit.</summary>
	public decimal? Multiplier { get; init; }
}

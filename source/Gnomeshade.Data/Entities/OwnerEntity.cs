﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a collection of other entities (users, roles, groups, etc.) that can own other entities.</summary>
/// <seealso cref="OwnershipEntity"/>
/// <seealso cref="UserEntity"/>
public sealed record OwnerEntity : Entity, INamedEntity
{
	/// <inheritdoc />
	public string Name { get; set; } = null!;

	/// <inheritdoc />
	public string NormalizedName { get; set; } = null!;
}

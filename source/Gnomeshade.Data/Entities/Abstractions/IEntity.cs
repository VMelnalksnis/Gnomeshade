// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>Represents an entity.</summary>
public interface IEntity
{
	/// <summary>Gets the unique id of the entity.</summary>
	public Guid Id { get; init; }

	/// <summary>Gets the timestamp of the creation of this entity.</summary>
	public Instant CreatedAt { get; init; }

	/// <summary>Gets the id of the user which created this entity.</summary>
	/// <seealso cref="UserEntity"/>
	public Guid CreatedByUserId { get; init; }

	/// <summary>Gets or sets the timestamp of the deletion of this entity.</summary>
	public Instant? DeletedAt { get; set; }

	/// <summary>Gets or sets the id of the user that created this entity.</summary>
	/// <seealso cref="UserEntity"/>
	public Guid? DeletedByUserId { get; set; }
}

﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>Represents an entity that can be modified.</summary>
/// <seealso cref="UserEntity"/>
public interface IModifiableEntity : IEntity
{
	/// <summary>Gets or sets the timestamp of the last modification of this entity.</summary>
	Instant ModifiedAt { get; set; }

	/// <summary>Gets or sets the id of the user which last modified this entity.</summary>
	/// <seealso cref="UserEntity"/>
	public Guid ModifiedByUserId { get; set; }
}

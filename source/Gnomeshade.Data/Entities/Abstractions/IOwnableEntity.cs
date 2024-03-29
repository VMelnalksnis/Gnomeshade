﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>Represents an entity that is owned by a <see cref="OwnerEntity"/>.</summary>
public interface IOwnableEntity : IEntity
{
	/// <summary>Gets or sets the id of the owner of this entity.</summary>
	/// <seealso cref="OwnerEntity"/>
	/// <seealso cref="OwnershipEntity"/>
	public Guid OwnerId { get; set; }
}

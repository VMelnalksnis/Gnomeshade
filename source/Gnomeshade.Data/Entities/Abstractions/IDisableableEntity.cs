// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>
/// An entity that can only be disabled instead of deleted because it is referenced by other entities.
/// </summary>
public interface IDisableableEntity : IEntity
{
	/// <summary>
	/// Gets or sets the point in time at which this entity was disabled.
	/// </summary>
	Instant? DisabledAt { get; set; }

	/// <summary>
	/// Gets or sets the id of the <see cref="UserEntity"/> which disabled the entity.
	/// </summary>
	Guid? DisabledByUserId { get; set; }
}

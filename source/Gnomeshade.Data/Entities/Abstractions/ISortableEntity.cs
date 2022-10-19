// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>Represents an entity that can have an explicit order.</summary>
public interface ISortableEntity : IEntity
{
	/// <summary>Gets or sets the order of the entity.</summary>
	public uint? Order { get; set; }
}

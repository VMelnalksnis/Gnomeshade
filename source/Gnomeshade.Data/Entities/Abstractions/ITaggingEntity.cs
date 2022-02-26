// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>Tags an entity of type <typeparamref name="TEntity"/> with a <see cref="TagEntity"/>.</summary>
/// <typeparam name="TEntity">The tagged entity type.</typeparam>
public interface ITaggingEntity<TEntity>
	where TEntity : class, IEntity
{
	/// <summary>
	/// Gets the timestamp of the creation of this entity.
	/// </summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>Gets the id of the <see cref="TagEntity"/>.</summary>
	public Guid TagId { get; init; }

	/// <summary>Gets the id of the tagged <typeparamref name="TEntity"/>.</summary>
	public Guid TaggedItemId { get; init; }
}

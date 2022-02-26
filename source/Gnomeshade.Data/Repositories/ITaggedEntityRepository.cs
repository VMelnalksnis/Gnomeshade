// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Repositories;

/// <summary>Defines methods for querying entities that can be tagged.</summary>
/// <typeparam name="TEntity">The tagged entity type.</typeparam>
public interface ITaggedEntityRepository<TEntity>
	where TEntity : IEntity
{
	/// <summary>Gets all entities with the specified tag.</summary>
	/// <param name="id">The tag by which to filter entities.</param>
	/// <param name="ownerId">The id of the owner of the entities.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>All entities with the specified tag.</returns>
	Task<IEnumerable<TEntity>> GetTaggedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);

	/// <summary>Tags the specified entity with the specified tag.</summary>
	/// <param name="id">The id of the entity to tag.</param>
	/// <param name="tagId">The id of the tag to add.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task<int> TagAsync(Guid id, Guid tagId, Guid ownerId);

	/// <summary>Removes the specified tags from the specified entity.</summary>
	/// <param name="id">The id of the entity to untag.</param>
	/// <param name="tagId">The id of the tag to remove.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task<int> UntagAsync(Guid id, Guid tagId, Guid ownerId);
}

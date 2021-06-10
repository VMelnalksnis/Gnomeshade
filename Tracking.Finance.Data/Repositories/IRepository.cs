// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Repositories
{
	public interface IRepository<TEntity>
		where TEntity : class, IEntity
	{
		Task<Guid> AddAsync(TEntity entity);

		Task<Guid> AddAsync(TEntity entity, IDbTransaction dbTransaction);

		Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

		Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		/// <param name="id">The id of the entity to delete.</param>
		/// <returns>The number of rows affected.</returns>
		Task<int> DeleteAsync(Guid id);
	}
}

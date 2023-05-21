// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities.Abstractions;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <inheritdoc />
public abstract class TransactionItemRepository<TEntity> : Repository<TEntity>
	where TEntity : Entity
{
	/// <summary>Initializes a new instance of the <see cref="TransactionItemRepository{TEntity}"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	protected TransactionItemRepository(ILogger<TransactionItemRepository<TEntity>> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <summary>Gets all entities of the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get the entities.</param>
	/// <param name="ownerId">The id of the owner of the entities.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all entities for the specified transaction.</returns>
	public abstract Task<IEnumerable<TEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		CancellationToken cancellationToken = default);

	/// <summary>Gets all entities of the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get the entities.</param>
	/// <param name="ownerId">The id of the owner of the entities.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>A collection of all entities for the specified transaction.</returns>
	public virtual Task<IEnumerable<TEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		DbTransaction dbTransaction)
	{
		throw new NotImplementedException();
	}
}

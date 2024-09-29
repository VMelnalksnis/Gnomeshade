// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data;

/// <summary>Transaction related actions spanning multiple entities.</summary>
public sealed class TransactionUnitOfWork
{
	private readonly TransactionRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="TransactionUnitOfWork"/> class.</summary>
	/// <param name="repository">The repository for managing transactions.</param>
	public TransactionUnitOfWork(TransactionRepository repository)
	{
		_repository = repository;
	}

	/// <summary>Adds a new transaction with the specified items.</summary>
	/// <param name="transaction">The transaction to create.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created transaction.</returns>
	/// <exception cref="ArgumentException"><paramref name="transaction"/> does not have any items.</exception>
	public async Task<Guid> AddAsync(TransactionEntity transaction, DbTransaction dbTransaction)
	{
		if (transaction.Id == Guid.Empty)
		{
			transaction = transaction with { Id = Guid.NewGuid() };
		}

		var transactionId = await _repository.AddAsync(transaction, dbTransaction).ConfigureAwait(false);
		return transactionId;
	}
}

﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

using JetBrains.Annotations;

namespace Gnomeshade.Data;

/// <summary>Transaction related actions spanning multiple entities.</summary>
public sealed class TransactionUnitOfWork
{
	private readonly DbConnection _dbConnection;
	private readonly TransactionRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="TransactionUnitOfWork"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	/// <param name="repository">The repository for managing transactions.</param>
	public TransactionUnitOfWork(DbConnection dbConnection, TransactionRepository repository)
	{
		_dbConnection = dbConnection;
		_repository = repository;
	}

	/// <summary>Adds a new transaction with the specified items.</summary>
	/// <param name="transaction">The transaction to create.</param>
	/// <returns>The id of the created transaction.</returns>
	public async Task<Guid> AddAsync(TransactionEntity transaction)
	{
		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();
		var transactionId = await AddAsync(transaction, dbTransaction);
		await dbTransaction.CommitAsync();
		return transactionId;
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

	/// <summary>Deletes the specified transaction and all its items.</summary>
	/// <param name="transaction">The transaction to delete.</param>
	/// <param name="userId">The id of the owner of the entity.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteAsync(TransactionEntity transaction, Guid userId)
	{
		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();
		if (await DeleteAsync(transaction, userId, dbTransaction) is not 1)
		{
			throw new InvalidOperationException("Failed to delete transaction");
		}

		await dbTransaction.CommitAsync();
	}

	/// <summary>
	/// Updates the specified transaction and all its items.
	/// </summary>
	/// <param name="transaction">The transaction to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="transaction"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateAsync(TransactionEntity transaction, UserEntity modifiedBy)
	{
		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();
		if (await UpdateAsync(transaction, modifiedBy, dbTransaction) is not 1)
		{
			throw new InvalidOperationException("Failed to update transaction");
		}

		await dbTransaction.CommitAsync();
	}

	[MustUseReturnValue]
	private Task<int> DeleteAsync(TransactionEntity transaction, Guid userId, DbTransaction dbTransaction)
	{
		return _repository.DeleteAsync(transaction.Id, userId, dbTransaction);
	}

	[MustUseReturnValue]
	private Task<int> UpdateAsync(TransactionEntity transaction, UserEntity modifiedBy, DbTransaction dbTransaction)
	{
		transaction.ModifiedByUserId = modifiedBy.Id;
		return _repository.UpdateAsync(transaction, dbTransaction);
	}
}

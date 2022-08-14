﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data;

/// <summary>Transaction related actions spanning multiple entities.</summary>
public sealed class TransactionUnitOfWork : IDisposable
{
	private readonly IDbConnection _dbConnection;
	private readonly TransactionRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="TransactionUnitOfWork"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	/// <param name="repository">The repository for managing transactions.</param>
	public TransactionUnitOfWork(IDbConnection dbConnection, TransactionRepository repository)
	{
		_dbConnection = dbConnection;
		_repository = repository;
	}

	/// <summary>Adds a new transaction with the specified items.</summary>
	/// <param name="transaction">The transaction to create.</param>
	/// <returns>The id of the created transaction.</returns>
	public async Task<Guid> AddAsync(TransactionEntity transaction)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

		try
		{
			var transactionId = await AddAsync(transaction, dbTransaction);
			dbTransaction.Commit();
			return transactionId;
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>Adds a new transaction with the specified items.</summary>
	/// <param name="transaction">The transaction to create.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created transaction.</returns>
	/// <exception cref="ArgumentException"><paramref name="transaction"/> does not have any items.</exception>
	public async Task<Guid> AddAsync(TransactionEntity transaction, IDbTransaction dbTransaction)
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
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteAsync(TransactionEntity transaction, Guid ownerId)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();
		try
		{
			await DeleteAsync(transaction, ownerId, dbTransaction);
			dbTransaction.Commit();
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>Deletes the specified transaction and all its items.</summary>
	/// <param name="transaction">The transaction to delete.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteAsync(TransactionEntity transaction, Guid ownerId, IDbTransaction dbTransaction)
	{
		await _repository.DeleteAsync(transaction.Id, ownerId, dbTransaction).ConfigureAwait(false);
	}

	/// <summary>
	/// Updates the specified transaction and all its items.
	/// </summary>
	/// <param name="transaction">The transaction to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="transaction"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateAsync(TransactionEntity transaction, UserEntity modifiedBy)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

		try
		{
			await UpdateAsync(transaction, modifiedBy, dbTransaction);
			dbTransaction.Commit();
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>Updates the specified transaction and all its items.</summary>
	/// <param name="transaction">The transaction to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="transaction"/>.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task UpdateAsync(TransactionEntity transaction, UserEntity modifiedBy, IDbTransaction dbTransaction)
	{
		transaction.ModifiedByUserId = modifiedBy.Id;
		return _repository.UpdateAsync(transaction, dbTransaction);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_dbConnection.Dispose();
		_repository.Dispose();
	}
}

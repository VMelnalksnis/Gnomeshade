// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data;

/// <summary>
/// Transaction related actions spanning multiple entities.
/// </summary>
public sealed class TransactionUnitOfWork : IDisposable
{
	private readonly IDbConnection _dbConnection;
	private readonly TransactionRepository _repository;
	private readonly TransactionItemRepository _itemRepository;

	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionUnitOfWork"/> class.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	/// <param name="repository">The repository for managing transactions.</param>
	/// <param name="itemRepository">The repository for managing transaction items.</param>
	public TransactionUnitOfWork(
		IDbConnection dbConnection,
		TransactionRepository repository,
		TransactionItemRepository itemRepository)
	{
		_dbConnection = dbConnection;
		_repository = repository;
		_itemRepository = itemRepository;
	}

	/// <summary>
	/// Adds a new transaction with the specified items.
	/// </summary>
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

	/// <summary>
	/// Adds a new transaction with the specified items.
	/// </summary>
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

		foreach (var transactionItem in transaction.Items)
		{
			var item = transactionItem with
			{
				Id = Guid.NewGuid(),
				OwnerId = transaction.OwnerId,
				CreatedByUserId = transaction.CreatedByUserId,
				ModifiedByUserId = transaction.ModifiedByUserId,
				TransactionId = transactionId,
			};

			_ = await _itemRepository.AddAsync(item, dbTransaction).ConfigureAwait(false);
		}

		return transactionId;
	}

	/// <summary>
	/// Deletes the specified transaction and all its items.
	/// </summary>
	/// <param name="transaction">The transaction to delete.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <returns>The number of affected rows.</returns>
	public async Task<int> DeleteAsync(TransactionEntity transaction, Guid ownerId)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();
		try
		{
			var rows = await DeleteAsync(transaction, ownerId, dbTransaction);
			dbTransaction.Commit();
			return rows;
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>
	/// Deletes the specified transaction and all its items.
	/// </summary>
	/// <param name="transaction">The transaction to delete.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public async Task<int> DeleteAsync(TransactionEntity transaction, Guid ownerId, IDbTransaction dbTransaction)
	{
		var rows = 0;
		foreach (var item in transaction.Items)
		{
			rows += await _itemRepository.DeleteAsync(item.Id, ownerId, dbTransaction).ConfigureAwait(false);
		}

		rows += await _repository.DeleteAsync(transaction.Id, ownerId, dbTransaction).ConfigureAwait(false);
		return rows;
	}

	/// <summary>
	/// Updates the specified transaction and all its items.
	/// </summary>
	/// <param name="transaction">The transaction to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="transaction"/>.</param>
	/// <returns>The number of affected rows.</returns>
	public async Task<int> UpdateAsync(TransactionEntity transaction, UserEntity modifiedBy)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

		try
		{
			var rows = await UpdateAsync(transaction, modifiedBy, dbTransaction);
			dbTransaction.Commit();
			return rows;
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>
	/// Updates the specified transaction and all its items.
	/// </summary>
	/// <param name="transaction">The transaction to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="transaction"/>.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public async Task<int> UpdateAsync(TransactionEntity transaction, UserEntity modifiedBy, IDbTransaction dbTransaction)
	{
		transaction.ModifiedByUserId = modifiedBy.Id;
		var rows = await _repository.UpdateAsync(transaction, dbTransaction).ConfigureAwait(false);

		foreach (var transactionItem in transaction.Items)
		{
			transactionItem.ModifiedByUserId = modifiedBy.Id;
			transactionItem.TransactionId = transaction.Id;
			rows += await _itemRepository.UpdateAsync(transactionItem, dbTransaction).ConfigureAwait(false);
		}

		return rows;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_dbConnection.Dispose();
		_repository.Dispose();
		_itemRepository.Dispose();
	}
}

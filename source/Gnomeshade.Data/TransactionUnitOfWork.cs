// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data
{
	public sealed class TransactionUnitOfWork : IDisposable
	{
		private readonly IDbConnection _dbConnection;
		private readonly TransactionRepository _repository;
		private readonly TransactionItemRepository _itemRepository;

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
		/// <param name="items">The items to add to the created transaction.</param>
		/// <returns>The id of the created transaction.</returns>
		public async Task<Guid> AddAsync(Transaction transaction, IReadOnlyCollection<TransactionItem> items)
		{
			if (!items.Any())
			{
				throw new ArgumentException("Transaction must have at least one item", nameof(items));
			}

			using var dbTransaction = BeginTransaction();
			try
			{
				var transactionId = await _repository.AddAsync(transaction, dbTransaction).ConfigureAwait(false);

				foreach (var transactionItem in items)
				{
					transactionItem.TransactionId = transactionId;
					_ = await _itemRepository.AddAsync(transactionItem, dbTransaction).ConfigureAwait(false);
				}

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
		/// Deletes the specified transaction and all its items.
		/// </summary>
		/// <param name="transaction">The transaction to delete.</param>
		/// <returns>The count of affected entities.</returns>
		public async Task<int> DeleteAsync(Transaction transaction)
		{
			using var dbTransaction = BeginTransaction();
			try
			{
				var affectedEntities = 0;
				foreach (var item in transaction.Items)
				{
					affectedEntities += await _itemRepository.DeleteAsync(item.Id, dbTransaction).ConfigureAwait(false);
				}

				affectedEntities += await _repository.DeleteAsync(transaction.Id, dbTransaction).ConfigureAwait(false);
				dbTransaction.Commit();

				return affectedEntities;
			}
			catch (Exception)
			{
				dbTransaction.Rollback();
				throw;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_itemRepository.Dispose();
		}

		private IDbTransaction BeginTransaction()
		{
			if (!_dbConnection.State.HasFlag(ConnectionState.Open))
			{
				_dbConnection.Open();
			}

			return _dbConnection.BeginTransaction();
		}
	}
}

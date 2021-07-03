// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class TransactionRepository : IDisposable, IRepository<Transaction>
	{
		private const string _insertSql =
			"INSERT INTO transactions (owner_id, created_by_user_id, modified_by_user_id, date, description, generated, validated, completed) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Date, @Description, @Generated, @Validated, @Completed) RETURNING id";

		private const string _selectSql =
			"SELECT id, owner_id OwnerId, created_at CreatedAt, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, date, description, generated \"Generated\", validated, completed FROM transactions";

		private const string _deleteSql = "DELETE FROM transactions WHERE id = @Id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public TransactionRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <summary>
		/// Adds a new transaction.
		/// </summary>
		/// <param name="transaction">The transaction to add.</param>
		/// <returns>The id of the created transaction.</returns>
		public async Task<Guid> AddAsync(Transaction transaction)
		{
			return await _dbConnection.QuerySingleAsync<Guid>(_insertSql, transaction).ConfigureAwait(false);
		}

		/// <summary>
		/// Adds a new transaction item.
		/// </summary>
		/// <param name="transaction">The transaction to add.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the created transaction.</returns>
		public async Task<Guid> AddAsync(Transaction transaction, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, transaction, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Searches for a transaction with the specified id.
		/// </summary>
		/// <param name="id">The id to search by.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="Transaction"/> if one exists, otherwise <see langword="null"/>.</returns>
		public async Task<Transaction?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleOrDefaultAsync<Transaction>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets a transaction with the specified id.
		/// </summary>
		/// <param name="id">The id to search by.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="Transaction"/> with the specified id.</returns>
		public async Task<Transaction> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleAsync<Transaction>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all transactions.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all transactions.</returns>
		public async Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var commandDefinition = new CommandDefinition(_selectSql, cancellationToken: cancellationToken);
			var transactionItems = await _dbConnection.QueryAsync<Transaction>(commandDefinition).ConfigureAwait(false);
			return transactionItems.ToList();
		}

		/// <inheritdoc />
		public async Task<int> DeleteAsync(Guid id)
		{
			return await _dbConnection.ExecuteAsync(_deleteSql, new { id }).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();
	}
}

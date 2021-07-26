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

using Gnomeshade.Core;
using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories.Extensions;

namespace Gnomeshade.Data.Repositories
{
	public sealed class TransactionRepository : IDisposable
	{
		private const string _insertSql =
			"INSERT INTO transactions (owner_id, created_by_user_id, modified_by_user_id, date, description, import_hash, imported_at, validated_at, validated_by_user_id) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Date, @Description, @ImportHash, @ImportedAt, @ValidatedAt, @ValidatedByUserId) RETURNING id";

		private const string _selectSql =
			"SELECT t.id, " +
			"t.owner_id OwnerId, " +
			"t.created_at CreatedAt, " +
			"t.created_by_user_id CreatedByUserId, " +
			"t.modified_at ModifiedAt, " +
			"t.modified_by_user_id ModifiedByUserId, " +
			"t.date, " +
			"t.description, " +
			"t.import_hash ImportHash, " +
			"t.imported_at ImportedAt, " +
			"t.validated_at ValidatedAt, " +
			"t.validated_by_user_id ValidatedByUserId, " +
			"ti.id, " +
			"ti.owner_id OwnerId, " +
			"ti.transaction_id TransactionId, " +
			"ti.source_amount SourceAmount, " +
			"ti.source_account_id SourceAccountId, " +
			"ti.target_amount TargetAmount, " +
			"ti.target_account_id TargetAccountId, " +
			"ti.created_at CreatedAt, " +
			"ti.created_by_user_id CreatedByUserId, " +
			"ti.modified_at ModifiedAt, " +
			"ti.modified_by_user_id ModifiedByUserId, " +
			"ti.product_id ProductId, " +
			"ti.amount, " +
			"ti.bank_reference BankReference, " +
			"ti.external_reference ExternalReference, " +
			"ti.internal_reference InternalReference, " +
			"ti.description, " +
			"ti.delivery_date DeliverDate, " +
			"p.id, " +
			"p.created_at CreatedAt, " +
			"p.owner_id OwnerId, " +
			"p.created_by_user_id CreatedByUserId, " +
			"p.modified_at ModifiedAt, " +
			"p.modified_by_user_id ModifiedByUserId, " +
			"p.name, " +
			"p.normalized_name NormalizedName, " +
			"p.description, " +
			"p.unit_id UnitId " +
			"FROM transactions t " +
			"INNER JOIN transaction_items ti ON t.id = ti.transaction_id " +
			"INNER JOIN products p ON ti.product_id = p.id";

		private const string _deleteSql = "DELETE FROM transactions WHERE id = @Id;";

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
		public Task<Transaction?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE t.id = @id LIMIT 2;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Searches for a transaction with the specified import hash.
		/// </summary>
		/// <param name="importHash">The <see cref="Sha512Value"/> of the transaction import source data.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="Transaction"/> if one exists, otherwise <see langword="null"/>.</returns>
		public Task<Transaction?> FindByImportHashAsync(
			byte[] importHash,
			CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE t.import_hash = @importHash LIMIT 2;";
			var command = new CommandDefinition(sql, new { importHash }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Gets a transaction with the specified id.
		/// </summary>
		/// <param name="id">The id to search by.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="Transaction"/> with the specified id.</returns>
		public async Task<Transaction> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE t.id = @id LIMIT 2;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			var groupedTransactions = await GetTransactionsAsync(command).ConfigureAwait(false);
			var grouping = groupedTransactions.Single();
			return Transaction.FromGrouping(grouping);
		}

		/// <summary>
		/// Gets all transactions which have their <see cref="Transaction.Date"/> within the specified period.
		/// </summary>
		/// <param name="from">The start of the time range.</param>
		/// <param name="to">The end of the time range.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all transactions.</returns>
		public async Task<List<Transaction>> GetAllAsync(
			DateTimeOffset from,
			DateTimeOffset to,
			CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE t.date >= @from AND t.date <= @to ORDER BY t.date DESC";
			var commandDefinition = new CommandDefinition(sql, new { from, to }, cancellationToken: cancellationToken);

			var groupedTransactions = await GetTransactionsAsync(commandDefinition).ConfigureAwait(false);
			return groupedTransactions.Select(grouping => Transaction.FromGrouping(grouping)).ToList();
		}

		public Task<int> DeleteAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_deleteSql, new { id }, dbTransaction);
			return _dbConnection.ExecuteAsync(command);
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();

		private async Task<IEnumerable<IGrouping<Transaction, OneToOne<Transaction, TransactionItem>>>>
			GetTransactionsAsync(CommandDefinition command)
		{
			var oneToOnes =
				await _dbConnection
					.QueryAsync<Transaction, TransactionItem, Product, OneToOne<Transaction, TransactionItem>>(
						command,
						(transaction, item, product) =>
						{
							item.Product = product;
							return new(transaction, item);
						})
					.ConfigureAwait(false);

			return oneToOnes.GroupBy(oneToOne => oneToOne.First);
		}

		private async Task<Transaction?> FindAsync(CommandDefinition command)
		{
			var groupedTransactions = await GetTransactionsAsync(command).ConfigureAwait(false);
			var grouping = groupedTransactions.SingleOrDefault();
			return grouping is null ? null : Transaction.FromGrouping(grouping);
		}
	}
}

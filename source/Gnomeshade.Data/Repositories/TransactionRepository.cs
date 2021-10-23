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
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories.Extensions;

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// Database backed <see cref="TransactionEntity"/> repository.
	/// </summary>
	public sealed class TransactionRepository : Repository<TransactionEntity>
	{
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
			"ti.delivery_date DeliveryDate, " +
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

		private const string _findSuffix = "WHERE t.id = @id;";
		private const string _findSingleSql = $"{_selectSql} {_findSuffix}";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public TransactionRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <inheritdoc />
		protected override string DeleteSql => "DELETE FROM transactions WHERE id = @id;";

		/// <inheritdoc />
		protected override string InsertSql =>
			"INSERT INTO transactions (owner_id, created_by_user_id, modified_by_user_id, date, description, import_hash, imported_at, validated_at, validated_by_user_id) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Date, @Description, @ImportHash, @ImportedAt, @ValidatedAt, @ValidatedByUserId) RETURNING id";

		/// <inheritdoc />
		protected override string SelectSql => _selectSql;

		/// <inheritdoc />
		protected override string UpdateSql =>
			"UPDATE transactions SET modified_at = DEFAULT, modified_by_user_id = @ModifiedByUserId, date = @Date, description = @Description, import_hash = @ImportHash, imported_at = @ImportedAt, validated_at = @ValidatedAt, validated_by_user_id = @ValidatedByUserId RETURNING id";

		/// <inheritdoc />
		protected override string FindSql => _findSuffix;

		/// <inheritdoc />
		public override Task<TransactionEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(_findSingleSql, new { id }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Searches for a transaction with the specified import hash.
		/// </summary>
		/// <param name="importHash">The <see cref="Sha512Value"/> of the transaction import source data.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="TransactionEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
		public Task<TransactionEntity?> FindByImportHashAsync(
			byte[] importHash,
			CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE t.import_hash = @importHash;";
			var command = new CommandDefinition(sql, new { importHash }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Searches for a transaction with the specified import hash using the specified database transaction.
		/// </summary>
		/// <param name="importHash">The <see cref="Sha512Value"/> of the transaction import source data.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The <see cref="TransactionEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
		public Task<TransactionEntity?> FindByImportHashAsync(
			byte[] importHash,
			IDbTransaction dbTransaction)
		{
			const string sql = _selectSql + " WHERE t.import_hash = @importHash;";
			var command = new CommandDefinition(sql, new { importHash }, dbTransaction);
			return FindAsync(command);
		}

		/// <inheritdoc />
		public override Task<TransactionEntity> GetByIdAsync(
			Guid id,
			CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(_findSingleSql, new { id }, cancellationToken: cancellationToken);
			return GetAsync(command);
		}

		/// <summary>
		/// Gets a transaction with the specified id using the specified database transaction.
		/// </summary>
		/// <param name="id">The id of the transaction to get.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The transaction with the specified id.</returns>
		public Task<TransactionEntity> GetByIdAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_findSingleSql, new { id }, dbTransaction);
			return GetAsync(command);
		}

		/// <summary>
		/// Gets all transactions which have their <see cref="TransactionEntity.Date"/> within the specified period.
		/// </summary>
		/// <param name="from">The start of the time range.</param>
		/// <param name="to">The end of the time range.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all transactions.</returns>
		public async Task<List<TransactionEntity>> GetAllAsync(
			DateTimeOffset from,
			DateTimeOffset to,
			CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE t.date >= @from AND t.date <= @to ORDER BY t.date DESC";
			var commandDefinition = new CommandDefinition(sql, new { from, to }, cancellationToken: cancellationToken);

			var transactions = await GetTransactionsAsync(commandDefinition).ConfigureAwait(false);
			return transactions.ToList();
		}

		private async Task<IEnumerable<TransactionEntity>> GetTransactionsAsync(CommandDefinition command)
		{
			var oneToOnes =
				await _dbConnection
					.QueryAsync<TransactionEntity, TransactionItemEntity, ProductEntity, OneToOne<TransactionEntity, TransactionItemEntity>>(
						command,
						(transaction, item, product) =>
						{
							item.Product = product;
							return new(transaction, item);
						})
					.ConfigureAwait(false);

			// ReSharper disable once ConvertClosureToMethodGroup
			return oneToOnes.GroupBy(oneToOne => oneToOne.First.Id).Select(grouping => TransactionEntity.FromGrouping(grouping));
		}

		private async Task<TransactionEntity?> FindAsync(CommandDefinition command)
		{
			var transactions = await GetTransactionsAsync(command).ConfigureAwait(false);
			return transactions.SingleOrDefault();
		}

		private async Task<TransactionEntity> GetAsync(CommandDefinition command)
		{
			var transactions = await GetTransactionsAsync(command).ConfigureAwait(false);
			return transactions.Single();
		}
	}
}

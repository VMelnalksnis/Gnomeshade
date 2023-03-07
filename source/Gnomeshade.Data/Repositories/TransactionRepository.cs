// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

using NodaTime;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="TransactionEntity"/> repository.</summary>
public sealed class TransactionRepository : Repository<TransactionEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionRepository"/> class with a database connection.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransactionRepository(DbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Transaction.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transaction.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transaction.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transaction.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE t.deleted_at IS NULL AND t.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "t.deleted_at IS NULL";

	/// <summary>Gets all transactions which have their <see cref="TransactionEntity.BookedAt"/> within the specified period.</summary>
	/// <param name="from">The start of the time range.</param>
	/// <param name="to">The end of the time range.</param>
	/// <param name="ownerId">The id of the owner of the transactions.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all transactions.</returns>
	public async Task<List<TransactionEntity>> GetAllAsync(
		Instant from,
		Instant to,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql =
			$"{SelectSql} WHERE t.deleted_at IS NULL AND (t.valued_at >= @from OR t.booked_at >= @from) AND (t.valued_at <= @to OR t.booked_at <= @to) AND {AccessSql} ORDER BY t.valued_at DESC";
		var command = new CommandDefinition(sql, new { from, to, ownerId }, cancellationToken: cancellationToken);

		var transactions = await GetEntitiesAsync(command).ConfigureAwait(false);
		return transactions.ToList();
	}

	public async Task<DetailedTransactionEntity?> FindDetailedAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = @$"{Queries.Transaction.SelectDetailed}
WHERE transactions.deleted_at IS NULL 
AND transactions.id = @id
AND {AccessSql} 
ORDER BY transactions.valued_at DESC";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		var transactions = await GetDetailedTransactions(command);
		return transactions.SingleOrDefault();
	}

	public Task<IEnumerable<DetailedTransactionEntity>> GetAllDetailedAsync(
		Instant from,
		Instant to,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = @$"{Queries.Transaction.SelectDetailed}
WHERE transactions.deleted_at IS NULL 
AND (transactions.valued_at >= @from OR transactions.booked_at >= @from) 
AND (transactions.valued_at <= @to OR transactions.booked_at <= @to) 
AND {AccessSql} 
ORDER BY transactions.valued_at DESC";
		var command = new CommandDefinition(sql, new { from, to, ownerId }, cancellationToken: cancellationToken);
		return GetDetailedTransactions(command);
	}

	/// <summary>Gets all links of the specified transaction.</summary>
	/// <param name="id">The id of the transaction for which to get all links.</param>
	/// <param name="ownerId">The id of the owner of the transaction and links.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>All links of the specified transaction.</returns>
	public Task<IEnumerable<LinkEntity>> GetAllLinksAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $@"{Queries.Link.Select}
         INNER JOIN transaction_links ON transaction_links.link_id = links.id
         WHERE transaction_links.transaction_id = @id
           AND {AccessSql};";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return DbConnection.QueryAsync<LinkEntity>(command);
	}

	/// <summary>Adds the specified link to the specified transaction.</summary>
	/// <param name="id">The id of the transaction to which to add the link.</param>
	/// <param name="linkId">The id of the link to add to the transaction.</param>
	/// <param name="ownerId">The id of the owner of the transaction and link.</param>
	/// <returns>The number of rows affected.</returns>
	public Task<int> AddLinkAsync(Guid id, Guid linkId, Guid ownerId)
	{
		const string sql = @"
INSERT INTO transaction_links 
    (created_at, created_by_user_id, link_id, transaction_id) 
VALUES 
    (CURRENT_TIMESTAMP, @ownerId, @linkId, @id);";

		var command = new CommandDefinition(sql, new { id, linkId, ownerId });
		return DbConnection.ExecuteAsync(command);
	}

	/// <summary>Removes the specified link to the specified transaction.</summary>
	/// <param name="id">The id of the transaction from which to remove the link.</param>
	/// <param name="linkId">The id of the link to remove from the transaction.</param>
	/// <param name="ownerId">The id of the owner of the transaction and link.</param>
	/// <returns>The number of rows affected.</returns>
	public Task<int> RemoveLinkAsync(Guid id, Guid linkId, Guid ownerId)
	{
		const string sql = @"
DELETE FROM transaction_links
WHERE transaction_links.transaction_id = @id
  AND transaction_links.link_id = @linkId;";

		var command = new CommandDefinition(sql, new { id, linkId, ownerId });
		return DbConnection.ExecuteAsync(command);
	}

	/// <summary>Merges one transaction into another.</summary>
	/// <param name="targetId">The id of the transaction into which to merge.</param>
	/// <param name="sourceId">The id of the transaction which to merge into the other one.</param>
	/// <param name="ownerId">The id of the owner of the transactions.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task MergeAsync(Guid targetId, Guid sourceId, Guid ownerId, DbTransaction dbTransaction)
	{
		var moveDetailsCommand =
			new CommandDefinition(Queries.Transaction.Merge, new { targetId, sourceId, ownerId }, dbTransaction);
		await DbConnection.ExecuteAsync(moveDetailsCommand);
		await DeleteAsync(sourceId, ownerId, dbTransaction);
	}

	public Task<IEnumerable<TransactionEntity>> GetRelatedAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = @$"WITH r AS (SELECT ""second"" FROM related_transactions WHERE related_transactions.first = @id)
{SelectSql}
WHERE t.id IN (SELECT ""second"" FROM r) AND {NotDeleted} AND {AccessSql};";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return DbConnection.QueryAsync<TransactionEntity>(command);
	}

	public async Task AddRelatedAsync(Guid id, Guid relatedId, Guid ownerId)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();
		var transaction = await FindByIdAsync(id, ownerId, dbTransaction, AccessLevel.Write);
		var relatedTransaction = await FindByIdAsync(relatedId, ownerId, dbTransaction);
		if (transaction is null || relatedTransaction is null)
		{
			throw new InvalidOperationException();
		}

		const string sql = "INSERT INTO related_transactions(first, second) VALUES (@id, @relatedId);";
		var command = new CommandDefinition(sql, new { id, relatedId }, dbTransaction);
		await DbConnection.ExecuteAsync(command);
		await dbTransaction.CommitAsync();
	}

	public async Task RemoveRelatedAsync(Guid id, Guid relatedId, Guid ownerId)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();
		var transaction = await FindByIdAsync(id, ownerId, dbTransaction, AccessLevel.Write);
		var relatedTransaction = await FindByIdAsync(relatedId, ownerId, dbTransaction);
		if (transaction is null || relatedTransaction is null)
		{
			throw new InvalidOperationException();
		}

		const string sql = "DELETE FROM related_transactions WHERE first = @id AND second = @relatedId;";
		var command = new CommandDefinition(sql, new { id, relatedId }, dbTransaction);
		await DbConnection.ExecuteAsync(command);
		await dbTransaction.CommitAsync();
	}

	private async Task<IEnumerable<DetailedTransactionEntity>> GetDetailedTransactions(CommandDefinition command)
	{
		var transactions = await DbConnection
			.QueryAsync<TransactionEntity, PurchaseEntity, TransferEntity, LoanEntity, LinkEntity,
				DetailedTransactionEntity>(
				command,
				(transaction, purchase, transfer, loan, link) =>
				{
					var detailed = new DetailedTransactionEntity(transaction);

					if (purchase is { DeletedAt: null })
					{
						detailed.Purchases.Add(purchase);
					}

					if (transfer is { DeletedAt: null })
					{
						detailed.Transfers.Add(transfer);
					}

					if (loan is { DeletedAt: null })
					{
						detailed.Loans.Add(loan);
					}

					if (link is { DeletedAt: null })
					{
						detailed.Links.Add(link);
					}

					return detailed;
				},
				splitOn: "Id,Id,Id,Id");

		return transactions
			.GroupBy(transaction => transaction.Id)
			.Select(grouping => new DetailedTransactionEntity(grouping.First())
			{
				Purchases = grouping.SelectMany(detailed => detailed.Purchases).DistinctBy(purchase => purchase.Id).ToList(),
				Transfers = grouping.SelectMany(detailed => detailed.Transfers).DistinctBy(transfer => transfer.Id).ToList(),
				Loans = grouping.SelectMany(detailed => detailed.Loans).DistinctBy(loan => loan.Id).ToList(),
				Links = grouping.SelectMany(detailed => detailed.Links).DistinctBy(link => link.Id).ToList(),
			});
	}
}

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
using Gnomeshade.Data.Logging;
using Gnomeshade.Data.Repositories.Extensions;

using Microsoft.Extensions.Logging;

using NodaTime;

using static Gnomeshade.Data.Repositories.AccessLevel;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="TransactionEntity"/> repository.</summary>
public sealed class TransactionRepository : Repository<TransactionEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionRepository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransactionRepository(ILogger<TransactionRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Transaction.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transaction.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Transaction.SelectAll;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transaction.Update;

	/// <inheritdoc />
	protected override string FindSql => "t.id = @id";

	protected override string GroupBy => "GROUP BY t.id";

	/// <inheritdoc />
	protected override string NotDeleted => "t.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transaction.Select;

	[Obsolete]
	public async Task<DetailedTransactionEntity?> FindDetailedAsync(
		Guid id,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.FindId(id);
		var transactions = await GetDetailedTransactions(new(
			@$"{Queries.Transaction.SelectDetailed} AND transactions.id = @id;",
			new { id, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));

		return transactions.SingleOrDefault();
	}

	[Obsolete]
	public Task<IEnumerable<DetailedTransactionEntity>> GetAllDetailedAsync(
		Instant from,
		Instant to,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetDetailedTransactions(new(
			@$"{Queries.Transaction.SelectDetailed}
  AND (transactions.id IN (SELECT t.transaction_id
						   FROM transfers t
						   WHERE (t.valued_at >= @from OR t.booked_at >= @from)
							 AND (t.valued_at <= @to OR t.booked_at <= @to))
	OR (transfers.valued_at IS NULL AND transfers.booked_at IS NULL))",
			new { from, to, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	public async Task<DetailedTransaction2Entity?> FindDetailed2Async(
		Guid id,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.FindId(id);
		var transactions = await GetDetailedTransactions2(new(
			$"{Queries.Transaction.SelectDetailed2} AND transactions.id = @id;",
			new { id, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));

		return transactions.SingleOrDefault();
	}

	public Task<IEnumerable<DetailedTransaction2Entity>> GetAllDetailed2Async(
		Instant from,
		Instant to,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetDetailedTransactions2(new(
			@$"{Queries.Transaction.SelectDetailed2}
  AND (transactions.id IN (SELECT t.transaction_id
						   FROM transfers t
						   WHERE (t.valued_at >= @from OR t.booked_at >= @from)
							 AND (t.valued_at <= @to OR t.booked_at <= @to))
	OR (transfers.valued_at IS NULL AND transfers.booked_at IS NULL))",
			new { from, to, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	public async Task<IEnumerable<LinkEntity>> GetAllLinksAsync(
		Guid id,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var entities = await DbConnection.QueryAsync<LinkEntity>(new(
			$"{Queries.Link.Select} AND transaction_links.transaction_id = @id GROUP BY links.id;",
			new { id, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));

		return entities.DistinctBy(entity => entity.Id);
	}

	public Task<int> AddLinkAsync(Guid id, Guid linkId, Guid userId)
	{
		const string sql = @"
INSERT INTO transaction_links 
    (created_at, created_by_user_id, link_id, transaction_id) 
VALUES 
    (CURRENT_TIMESTAMP, @userId, @linkId, @id);";

		var command = new CommandDefinition(sql, new { id, linkId, userId });
		return DbConnection.ExecuteAsync(command);
	}

	public Task<int> RemoveLinkAsync(Guid id, Guid linkId, Guid userId)
	{
		const string sql = @"
DELETE FROM transaction_links
WHERE transaction_links.transaction_id = @id
  AND transaction_links.link_id = @linkId;";

		var command = new CommandDefinition(sql, new { id, linkId, userId });
		return DbConnection.ExecuteAsync(command);
	}

	/// <summary>Merges one transaction into another.</summary>
	/// <param name="targetId">The id of the transaction into which to merge.</param>
	/// <param name="sourceId">The id of the transaction which to merge into the other one.</param>
	/// <param name="userId">The id of the owner of the transactions.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task MergeAsync(Guid targetId, Guid sourceId, Guid userId, DbTransaction dbTransaction)
	{
		Logger.MergeTransactions(sourceId, targetId);
		var moveDetailsCommand = new CommandDefinition(Queries.Transaction.Merge, new { targetId, sourceId, userId }, dbTransaction);
		await DbConnection.ExecuteAsync(moveDetailsCommand);
		if (await DeleteAsync(sourceId, userId, dbTransaction) is not 1)
		{
			throw new InvalidOperationException("Failed to delete merged source transaction");
		}
	}

	public Task<IEnumerable<TransactionEntity>> GetRelatedAsync(
		Guid id,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		return DbConnection.QueryAsync<TransactionEntity>(new(
			$"""
WITH r AS (SELECT "second" FROM related_transactions WHERE related_transactions.first = @id)
{SelectActiveSql}
AND t.id IN (SELECT "second" FROM r)
{GroupBy};
""",
			new { id, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	public async Task AddRelatedAsync(Guid id, Guid relatedId, Guid userId)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();
		var transaction = await FindByIdAsync(id, userId, dbTransaction, Write);
		var relatedTransaction = await FindByIdAsync(relatedId, userId, dbTransaction);
		if (transaction is null || relatedTransaction is null)
		{
			throw new InvalidOperationException();
		}

		const string sql = "INSERT INTO related_transactions(first, second) VALUES (@id, @relatedId);";
		var command = new CommandDefinition(sql, new { id, relatedId }, dbTransaction);
		await DbConnection.ExecuteAsync(command);
		await dbTransaction.CommitAsync();
	}

	public async Task RemoveRelatedAsync(Guid id, Guid relatedId, Guid userId)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();
		var transaction = await FindByIdAsync(id, userId, dbTransaction, Write);
		var relatedTransaction = await FindByIdAsync(relatedId, userId, dbTransaction);
		if (transaction is null || relatedTransaction is null)
		{
			throw new InvalidOperationException();
		}

		const string sql = "DELETE FROM related_transactions WHERE first = @id AND second = @relatedId;";
		var command = new CommandDefinition(sql, new { id, relatedId }, dbTransaction);
		await DbConnection.ExecuteAsync(command);
		await dbTransaction.CommitAsync();
	}

	[Obsolete]
	private async Task<IEnumerable<DetailedTransactionEntity>> GetDetailedTransactions(CommandDefinition command)
	{
#pragma warning disable CS0612 // Type or member is obsolete
		var transactions = await DbConnection
			.QueryAsync<TransactionEntity, PurchaseEntity, IdContainer?, TransferEntity, LoanEntity, LinkEntity,
				DetailedTransactionEntity>(
				command,
				(transaction, purchase, container, transfer, loan, link) =>
				{
					var detailed = new DetailedTransactionEntity(transaction);

					if (purchase is { DeletedAt: null })
					{
						detailed.Purchases.Add(purchase);
						if (container is not null)
						{
							purchase.ProjectIds = [container.Id];
						}
					}

					if (transfer is { DeletedAt: null })
					{
						detailed.Transfers.Add(transfer);
						detailed.BookedAt = transfer.BookedAt;
						detailed.ValuedAt = transfer.ValuedAt;
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
			.Select(grouping => new DetailedTransactionEntity(grouping.First() with
			{
				BookedAt = grouping.Select(transaction => transaction.BookedAt).Max(),
				ValuedAt = grouping.Select(transaction => transaction.ValuedAt).Max(),
			})
			{
				Purchases = grouping
					.SelectMany(detailed => detailed.Purchases)
					.GroupBy(purchase => purchase.Id)
					.Select(purchases =>
					{
						var purchase = purchases.First();
						purchase.ProjectIds = purchases.SelectMany(entity => entity.ProjectIds).Distinct().ToList();
						return purchase;
					})
					.ToList(),
				Transfers = grouping.SelectMany(detailed => detailed.Transfers).DistinctBy(transfer => transfer.Id)
					.ToList(),
				Loans = grouping.SelectMany(detailed => detailed.Loans).DistinctBy(loan => loan.Id).ToList(),
				Links = grouping.SelectMany(detailed => detailed.Links).DistinctBy(link => link.Id).ToList(),
			});
#pragma warning restore CS0612 // Type or member is obsolete
	}

	private async Task<IEnumerable<DetailedTransaction2Entity>> GetDetailedTransactions2(CommandDefinition command)
	{
		var transactions = await DbConnection
			.QueryAsync<TransactionEntity, PurchaseEntity, IdContainer?, TransferEntity, LoanPaymentEntity, LinkEntity, DetailedTransaction2Entity>(
				command,
				(transaction, purchase, container, transfer, payment, link) =>
				{
					var detailed = new DetailedTransaction2Entity(transaction);

					if (purchase is { DeletedAt: null })
					{
						detailed.Purchases.Add(purchase);
						if (container is not null)
						{
							purchase.ProjectIds = [container.Id];
						}
					}

					if (transfer is { DeletedAt: null })
					{
						detailed.Transfers.Add(transfer);
						detailed.BookedAt = transfer.BookedAt;
						detailed.ValuedAt = transfer.ValuedAt;
					}

					if (payment is { DeletedAt: null })
					{
						detailed.LoanPayments.Add(payment);
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
			.Select(grouping => new DetailedTransaction2Entity(grouping.First() with
			{
				BookedAt = grouping.Select(transaction => transaction.BookedAt).Max(),
				ValuedAt = grouping.Select(transaction => transaction.ValuedAt).Max(),
			})
			{
				Purchases = grouping
					.SelectMany(detailed => detailed.Purchases)
					.GroupBy(purchase => purchase.Id)
					.Select(purchases =>
					{
						var purchase = purchases.First();
						purchase.ProjectIds = purchases.SelectMany(entity => entity.ProjectIds).Distinct().ToList();
						return purchase;
					})
					.ToList(),
				Transfers = grouping.SelectMany(detailed => detailed.Transfers).DistinctBy(transfer => transfer.Id).ToList(),
				LoanPayments = grouping.SelectMany(detailed => detailed.LoanPayments).DistinctBy(paymentEntity => paymentEntity.Id).ToList(),
				Links = grouping.SelectMany(detailed => detailed.Links).DistinctBy(link => link.Id).ToList(),
			});
	}
}

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

using NodaTime;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="TransactionEntity"/> repository.</summary>
public sealed class TransactionRepository : Repository<TransactionEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionRepository"/> class with a database connection.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransactionRepository(IDbConnection dbConnection)
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
	protected override string FindSql => "WHERE t.id = @id";

	/// <summary>Searches for a transaction with the specified import hash using the specified database transaction.</summary>
	/// <param name="importHash">The <see cref="Sha512Value"/> of the transaction import source data.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The <see cref="TransactionEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TransactionEntity?> FindByImportHashAsync(
		byte[] importHash,
		Guid ownerId,
		IDbTransaction dbTransaction)
	{
		var sql = $"{SelectSql} WHERE t.import_hash = @importHash AND {AccessSql};";
		var command = new CommandDefinition(sql, new { importHash, ownerId }, dbTransaction);
		return FindAsync(command);
	}

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
			$"{SelectSql} WHERE (t.valued_at >= @from OR t.booked_at >= @from) AND (t.valued_at <= @to OR t.booked_at <= @to) AND {AccessSql} ORDER BY t.valued_at DESC";
		var command = new CommandDefinition(sql, new { from, to, ownerId }, cancellationToken: cancellationToken);

		var transactions = await GetEntitiesAsync(command).ConfigureAwait(false);
		return transactions.ToList();
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
    (DEFAULT, @ownerId, @linkId, @id);";

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
}

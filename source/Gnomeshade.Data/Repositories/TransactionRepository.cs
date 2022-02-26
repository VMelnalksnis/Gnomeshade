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

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="TransactionEntity"/> repository.
/// </summary>
public sealed class TransactionRepository : Repository<TransactionEntity>
{
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
	protected override string DeleteSql => Queries.Transaction.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transaction.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transaction.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transaction.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE t.id = @id";

	/// <summary>
	/// Searches for a transaction with the specified import hash.
	/// </summary>
	/// <param name="importHash">The <see cref="Sha512Value"/> of the transaction import source data.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The <see cref="TransactionEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TransactionEntity?> FindByImportHashAsync(
		byte[] importHash,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE t.import_hash = @importHash AND {_accessSql};";
		var command = new CommandDefinition(sql, new { importHash, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>
	/// Searches for a transaction with the specified import hash using the specified database transaction.
	/// </summary>
	/// <param name="importHash">The <see cref="Sha512Value"/> of the transaction import source data.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The <see cref="TransactionEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TransactionEntity?> FindByImportHashAsync(
		byte[] importHash,
		Guid ownerId,
		IDbTransaction dbTransaction)
	{
		var sql = $"{SelectSql} WHERE t.import_hash = @importHash AND {_accessSql};";
		var command = new CommandDefinition(sql, new { importHash, ownerId }, dbTransaction);
		return FindAsync(command);
	}

	/// <summary>
	/// Gets all transactions which have their <see cref="TransactionEntity.Date"/> within the specified period.
	/// </summary>
	/// <param name="from">The start of the time range.</param>
	/// <param name="to">The end of the time range.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all transactions.</returns>
	public async Task<List<TransactionEntity>> GetAllAsync(
		DateTimeOffset from,
		DateTimeOffset to,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE t.date >= @from AND t.date <= @to AND {_accessSql} ORDER BY t.date DESC";
		var command = new CommandDefinition(sql, new { from, to, ownerId }, cancellationToken: cancellationToken);

		var transactions = await GetEntitiesAsync(command).ConfigureAwait(false);
		return transactions.ToList();
	}

	/// <inheritdoc />
	protected override async Task<IEnumerable<TransactionEntity>> GetEntitiesAsync(CommandDefinition command)
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
}

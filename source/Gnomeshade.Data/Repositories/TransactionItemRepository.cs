// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="TransactionItemEntity"/> repository.
/// </summary>
public sealed class TransactionItemRepository :
	Repository<TransactionItemEntity>,
	ITaggedEntityRepository<TransactionItemEntity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionItemRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransactionItemRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.TransactionItem.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.TransactionItem.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.TransactionItem.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.TransactionItem.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE ti.id = @id";

	/// <inheritdoc />
	public Task<IEnumerable<TransactionItemEntity>> GetTaggedAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $@"{SelectSql} 
INNER JOIN transaction_item_tags ON transaction_item_tags.tagged_item_id = ti.id
INNER JOIN tags ON tags.id = transaction_item_tags.tag_id
WHERE tags.id = @id AND {_accessSql}";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return DbConnection.QueryAsync<TransactionItemEntity>(command);
	}

	/// <inheritdoc />
	public Task<int> TagAsync(Guid id, Guid tagId, Guid ownerId)
	{
		var command = new CommandDefinition(Queries.TransactionItem.AddTag, new { id, tagId, ownerId });
		return DbConnection.ExecuteAsync(command);
	}

	/// <inheritdoc />
	public Task<int> UntagAsync(Guid id, Guid tagId, Guid ownerId)
	{
		var command = new CommandDefinition(Queries.TransactionItem.RemoveTag, new { id, tagId, ownerId });
		return DbConnection.ExecuteAsync(command);
	}
}

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

/// <summary>Database backed <see cref="TagEntity"/> repository.</summary>
public sealed class TagRepository : NamedRepository<TagEntity>, ITaggedEntityRepository<TagEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TagRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TagRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => throw new NotImplementedException();

	/// <inheritdoc />
	protected override string InsertSql => Queries.Tag.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Tag.Select;

	/// <inheritdoc />
	protected override string UpdateSql => throw new NotImplementedException();

	/// <inheritdoc />
	protected override string FindSql => "WHERE t.id = @id";

	/// <inheritdoc />
	protected override string NameSql => "WHERE t.normalized_name = @name";

	/// <inheritdoc />
	public Task<IEnumerable<TagEntity>> GetTaggedAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $@"{SelectSql} 
INNER JOIN tag_tags ON tag_tags.tagged_item_id = t.id
INNER JOIN tags ON tags.id = tag_tags.tag_id
WHERE tags.id = @id AND {_accessSql}";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return DbConnection.QueryAsync<TagEntity>(command);
	}

	/// <inheritdoc />
	public Task<IEnumerable<TagEntity>> GetTagsAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $@"{Queries.Tag.Select}
INNER JOIN tag_tags ON tag_tags.tag_id = t.id
WHERE tag_tags.tagged_item_id = @id AND {_accessSql}";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return DbConnection.QueryAsync<TagEntity>(command);
	}

	/// <inheritdoc />
	public Task<int> TagAsync(Guid id, Guid tagId, Guid ownerId)
	{
		var command = new CommandDefinition(Queries.Tag.AddTag, new { id, tagId, ownerId });
		return DbConnection.ExecuteAsync(command);
	}

	/// <inheritdoc />
	public Task<int> UntagAsync(Guid id, Guid tagId, Guid ownerId)
	{
		var command = new CommandDefinition(Queries.Tag.RemoveTag, new { id, tagId, ownerId });
		return DbConnection.ExecuteAsync(command);
	}
}

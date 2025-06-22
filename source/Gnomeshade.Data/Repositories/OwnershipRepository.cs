// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="OwnershipEntity"/>.</summary>
public sealed class OwnershipRepository(ILogger<OwnershipRepository> logger, DbConnection dbConnection)
	: Repository<OwnershipEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.Ownership.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Ownership.Insert;

	/// <inheritdoc />
	protected override string TableName => "ownerships o";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Ownership.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Ownership.Update;

	/// <inheritdoc />
	protected override string FindSql => "o.id = @id";

	protected override string GroupBy => "GROUP BY o.id";

	/// <inheritdoc />
	protected override string NotDeleted => "1 = 1";

	public async Task AddDefaultAsync(Guid id, DbTransaction dbTransaction)
	{
		Logger.AddingEntityWithTransaction();
		const string text = "SELECT id AS Id FROM access WHERE normalized_name = 'OWNER';";
		var accessCommand = new CommandDefinition(text, null, dbTransaction);
		var accessId = await DbConnection.QuerySingleAsync<Guid>(accessCommand);

		await AddAsync(new() { Id = id, OwnerId = id, UserId = id, AccessId = accessId }, dbTransaction);
	}
}

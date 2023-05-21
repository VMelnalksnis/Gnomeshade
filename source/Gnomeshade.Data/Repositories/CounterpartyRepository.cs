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

/// <summary>Database backed <see cref="CounterpartyEntity"/> repository.</summary>
public sealed class CounterpartyRepository : NamedRepository<CounterpartyEntity>
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyRepository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public CounterpartyRepository(ILogger<CounterpartyRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string AccessSql => $"(({base.AccessSql}) OR \"AspNetUsers\".\"Id\" = c.id)";

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Counterparty.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Counterparty.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Counterparty.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Counterparty.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE c.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "c.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "WHERE c.normalized_name = upper(@name)";

	/// <summary>Merges one counterparty into another.</summary>
	/// <param name="targetId">The id of the counterparty into which to merge.</param>
	/// <param name="sourceId">The id of the counterparty which to merge into the other one.</param>
	/// <param name="ownerId">The id of the owner of the counterparties.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task MergeAsync(Guid targetId, Guid sourceId, Guid ownerId, DbTransaction dbTransaction)
	{
		Logger.MergeCounterparties(sourceId, targetId);
		var mergeCommand = new CommandDefinition(Queries.Counterparty.Merge, new { targetId, sourceId, ownerId }, dbTransaction);
		await DbConnection.ExecuteAsync(mergeCommand);
		await DeleteAsync(sourceId, ownerId, dbTransaction);
	}
}

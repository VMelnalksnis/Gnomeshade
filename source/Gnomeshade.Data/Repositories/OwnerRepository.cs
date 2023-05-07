// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="OwnerEntity"/>.</summary>
public sealed class OwnerRepository : NamedRepository<OwnerEntity>
{
	/// <summary>Initializes a new instance of the <see cref="OwnerRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public OwnerRepository(ILogger<OwnerRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => throw new NotImplementedException();

	/// <inheritdoc />
	protected override string InsertSql => Queries.Owner.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Owner.Select;

	/// <inheritdoc />
	protected override string UpdateSql => throw new NotImplementedException();

	protected override string FindSql => "WHERE o.id = @id";

	/// <inheritdoc />
	protected override string AccessSql => "users.id = @ownerId";

	protected override string NotDeleted => "o.deleted_at IS NULL";

	public Task AddDefaultAsync(Guid id, DbTransaction dbTransaction)
	{
		return AddAsync(new() { Id = id, Name = "Private", CreatedByUserId = id }, dbTransaction);
	}

	protected override async Task<IEnumerable<OwnerEntity>> GetEntitiesAsync(CommandDefinition command)
	{
		var owners = await DbConnection.QueryAsync<OwnerEntity>(command);
		return owners.DistinctBy(owner => owner.Id).ToList();
	}
}

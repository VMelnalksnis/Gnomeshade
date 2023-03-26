// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="OwnerEntity"/>.</summary>
public sealed class OwnerRepository
{
	private const string _insertWithIdSql = "INSERT INTO owners (id) VALUES (@id) RETURNING id;";
	private const string _selectSql = "SELECT id, created_at CreatedAt FROM owners";

	private readonly ILogger<OwnerRepository> _logger;
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="OwnerRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public OwnerRepository(ILogger<OwnerRepository> logger, DbConnection dbConnection)
	{
		_logger = logger;
		_dbConnection = dbConnection;
	}

	/// <summary>Adds a new owner with the specified <see cref="Entity.Id"/>.</summary>
	/// <param name="id">The id with which to create the entity.</param>
	/// <returns>The id of the new entity.</returns>
	public Task<Guid> AddAsync(Guid id)
	{
		_logger.AddingEntity();
		var command = new CommandDefinition(_insertWithIdSql, new { id });
		return _dbConnection.QuerySingleAsync<Guid>(command);
	}

	/// <summary>Adds a new owner with the specified <see cref="Entity.Id"/>.</summary>
	/// <param name="id">The id with which to create the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the new entity.</returns>
	public Task<Guid> AddAsync(Guid id, IDbTransaction dbTransaction)
	{
		_logger.AddingEntityWithTransaction();
		var command = new CommandDefinition(_insertWithIdSql, new { id }, dbTransaction);
		return _dbConnection.QuerySingleAsync<Guid>(command);
	}

	/// <summary>Gets all owners.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all owners.</returns>
	public Task<IEnumerable<OwnerEntity>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		_logger.GetAll();
		return _dbConnection.QueryAsync<OwnerEntity>(new(_selectSql, cancellationToken: cancellationToken));
	}

	/// <summary>Deletes the owner with the specified id.</summary>
	/// <param name="id">The id of the owner to delete.</param>
	/// <returns>The number of affected rows.</returns>
	public Task<int> DeleteAsync(Guid id)
	{
		_logger.DeletingEntity(id);
		return _dbConnection.ExecuteAsync("DELETE FROM owners WHERE id = @id", new { id });
	}
}

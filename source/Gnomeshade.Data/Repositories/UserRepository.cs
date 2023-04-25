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
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="UserEntity"/> repository.</summary>
public sealed class UserRepository
{
	private readonly ILogger<UserRepository> _logger;
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="UserRepository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public UserRepository(ILogger<UserRepository> logger, DbConnection dbConnection)
	{
		_logger = logger;
		_dbConnection = dbConnection;
	}

	/// <summary>Adds a new user with the specified values, including id.</summary>
	/// <param name="entity">The values to insert.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created user.</returns>
	public Task<Guid> AddWithIdAsync(UserEntity entity, IDbTransaction dbTransaction)
	{
		_logger.AddingEntityWithTransaction();
		var command = new CommandDefinition(Queries.User.Insert, entity, dbTransaction);
		return _dbConnection.QuerySingleAsync<Guid>(command);
	}

	/// <summary>Searches for a user with the specified id.</summary>
	/// <param name="id">The id to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The <see cref="UserEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
	public Task<UserEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		_logger.FindId(id);
		var commandDefinition = new CommandDefinition(Queries.User.Select, new { id }, cancellationToken: cancellationToken);
		return _dbConnection.QuerySingleOrDefaultAsync<UserEntity>(commandDefinition)!;
	}

	/// <summary>Updates the specified user.</summary>
	/// <param name="user">The user to update with the new information.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public Task<int> UpdateAsync(UserEntity user, IDbTransaction dbTransaction)
	{
		_logger.UpdatingEntityWithTransaction();
		var command = new CommandDefinition(Queries.User.Update, user, dbTransaction);
		return _dbConnection.ExecuteAsync(command);
	}

	public Task<IEnumerable<UserEntity>> Get(CancellationToken cancellationToken = default)
	{
		_logger.GetAll();
		var command = new CommandDefinition(
			"SELECT id, created_at CreatedAt, counterparty_id CounterpartyId FROM users;",
			cancellationToken: cancellationToken);
		return _dbConnection.QueryAsync<UserEntity>(command);
	}
}

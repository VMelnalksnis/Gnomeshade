// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
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
	public Task<Guid> AddWithIdAsync(UserEntity entity, DbTransaction dbTransaction)
	{
		_logger.AddingEntityWithTransaction();
		return _dbConnection.QuerySingleAsync<Guid>(new(Queries.User.Insert, entity, dbTransaction));
	}

	/// <summary>Updates the specified user.</summary>
	/// <param name="user">The user to update with the new information.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public Task<int> UpdateAsync(UserEntity user, DbTransaction dbTransaction)
	{
		_logger.UpdatingEntityWithTransaction();
		return _dbConnection.ExecuteAsync(new(Queries.User.Update, user, dbTransaction));
	}

	public Task<IEnumerable<UserEntity>> Get(CancellationToken cancellationToken = default)
	{
		_logger.GetAll();
		return _dbConnection.QueryAsync<UserEntity>(new(Queries.User.SelectAll, cancellationToken: cancellationToken));
	}

	public Task<UserEntity?> FindById(Guid id, CancellationToken cancellationToken = default)
	{
		_logger.FindId(id);
		return _dbConnection.QuerySingleOrDefaultAsync<UserEntity?>(new(
			$"{Queries.User.SelectAll} WHERE id = @id;",
			new { id },
			cancellationToken: cancellationToken));
	}
}

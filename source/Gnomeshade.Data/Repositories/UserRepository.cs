// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="UserEntity"/> repository.
/// </summary>
public sealed class UserRepository : IDisposable
{
	private readonly IDbConnection _dbConnection;

	/// <summary>
	/// Initializes a new instance of the <see cref="UserRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public UserRepository(IDbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	/// <summary>
	/// Adds a new user with the specified values, including id.
	/// </summary>
	/// <param name="entity">The values to insert.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created user.</returns>
	public Task<Guid> AddWithIdAsync(UserEntity entity, IDbTransaction dbTransaction)
	{
		var command = new CommandDefinition(Queries.User.Insert, entity, dbTransaction);
		return _dbConnection.QuerySingleAsync<Guid>(command);
	}

	/// <summary>
	/// Searches for a user with the specified id.
	/// </summary>
	/// <param name="id">The id to search by.</param>
	/// <returns>The <see cref="UserEntity"/> if one exists, otherwise <see langword="null"/>.</returns>
	public Task<UserEntity?> FindByIdAsync(Guid id)
	{
		var commandDefinition = new CommandDefinition(Queries.User.Select, new { id });
		return _dbConnection.QuerySingleOrDefaultAsync<UserEntity>(commandDefinition)!;
	}

	/// <summary>
	/// Updates the specified user.
	/// </summary>
	/// <param name="user">The user to update with the new information.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public Task<int> UpdateAsync(UserEntity user, IDbTransaction dbTransaction)
	{
		var command = new CommandDefinition(Queries.User.Update, user, dbTransaction);
		return _dbConnection.ExecuteAsync(command);
	}

	/// <inheritdoc/>
	public void Dispose() => _dbConnection.Dispose();
}

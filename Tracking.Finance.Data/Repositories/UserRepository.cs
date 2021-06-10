// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
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
		public async Task<Guid> AddWithIdAsync(User entity, IDbTransaction dbTransaction)
		{
			const string sql = "INSERT INTO users (id, counterparty_id) VALUES (@Id, @CounterpartyId) RETURNING id";
			var command = new CommandDefinition(sql, entity, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command);
		}

		/// <summary>
		/// Searches for a user with the specified id.
		/// </summary>
		/// <param name="id">The id to search by.</param>
		/// <returns>The <see cref="User"/> if one exists, otherwise <see langword="null"/>.</returns>
		public async Task<User?> FindByIdAsync(Guid id)
		{
			const string sql = "SELECT id, created_at, counterparty_id FROM users WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id });
			return await _dbConnection.QuerySingleOrDefaultAsync<User>(commandDefinition);
		}

		/// <inheritdoc/>
		public void Dispose() => _dbConnection.Dispose();
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class OwnerRepository : IDisposable
	{
		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="OwnerRepository"/> class.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public OwnerRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <summary>
		/// Adds a new owner.
		/// </summary>
		/// <returns>The id of the new entity.</returns>
		public async Task<Guid> AddAsync()
		{
			const string sql = "INSERT INTO owners VALUES (DEFAULT) RETURNING id";
			return await _dbConnection.QuerySingleAsync<Guid>(sql);
		}

		/// <summary>
		/// Adds a new owner with the specified <see cref="Owner.Id"/>.
		/// </summary>
		/// <param name="id">The id with which to create the entity.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the new entity.</returns>
		public async Task<Guid> AddAsync(Guid id, IDbTransaction dbTransaction)
		{
			const string sql = "INSERT INTO owners (id) VALUES (@Id) RETURNING id";
			var command = new CommandDefinition(sql, new { id }, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command);
		}

		/// <summary>
		/// Gets all owners.
		/// </summary>
		/// <returns>A collection of all owners</returns>
		public async Task<List<Owner>> GetAllAsync()
		{
			const string? sql = "SELECT id Id, created_at CreatedAt FROM owners";
			var entities = await _dbConnection.QueryAsync<Owner>(sql);
			return entities.ToList();
		}

		/// <inheritdoc/>
		public void Dispose() => _dbConnection.Dispose();
	}
}

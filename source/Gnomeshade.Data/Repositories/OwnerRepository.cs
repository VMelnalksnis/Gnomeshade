// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Models;

namespace Gnomeshade.Data.Repositories
{
	public sealed class OwnerRepository : IDisposable
	{
		private const string _insertSql = "INSERT INTO owners VALUES (DEFAULT) RETURNING id";
		private const string _insertWithIdSql = "INSERT INTO owners (id) VALUES (@Id) RETURNING id";
		private const string _selectSql = "SELECT id, created_at CreatedAt FROM owners";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="OwnerRepository"/> class with a database connection.
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
			return await _dbConnection.QuerySingleAsync<Guid>(_insertSql).ConfigureAwait(false);
		}

		/// <summary>
		/// Adds a new owner with the specified <see cref="Owner.Id"/>.
		/// </summary>
		/// <param name="id">The id with which to create the entity.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the new entity.</returns>
		public async Task<Guid> AddAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertWithIdSql, new { id }, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all owners.
		/// </summary>
		/// <returns>A collection of all owners.</returns>
		public async Task<List<Owner>> GetAllAsync()
		{
			var entities = await _dbConnection.QueryAsync<Owner>(_selectSql).ConfigureAwait(false);
			return entities.ToList();
		}

		/// <inheritdoc/>
		public void Dispose() => _dbConnection.Dispose();
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Models;

namespace Gnomeshade.Data.Repositories
{
	public sealed class OwnershipRepository
	{
		private const string _insertSql =
			"INSERT INTO ownerships (id, owner_id, user_id) VALUES (@Id, @Id, @Id) RETURNING id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="OwnershipRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public OwnershipRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <summary>
		/// Adds the default <see cref="Ownership"/>, where the <see cref="Ownership.Id"/>,
		/// <see cref="Ownership.OwnerId"/> and <see cref="Ownership.UserId"/> is the id of the user.
		/// </summary>
		/// <param name="id">Id of the user.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the created ownership.</returns>
		public async Task<Guid> AddDefaultAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, new { id }, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}
	}
}

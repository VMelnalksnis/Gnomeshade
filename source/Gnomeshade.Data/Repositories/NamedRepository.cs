// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// A base class for <see cref="INamedEntity"/> implementing queries based on entity name.
	/// </summary>
	/// <typeparam name="TNamedEntity">The type of entity that will be queried with this repository.</typeparam>
	public abstract class NamedRepository<TNamedEntity> : Repository<TNamedEntity>
		where TNamedEntity : class, INamedEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NamedRepository{TNamedEntity}"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		protected NamedRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <summary>
		/// Finds an entity by its normalized name.
		/// </summary>
		/// <param name="name">The normalized name of the entity to find.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
		public virtual Task<TNamedEntity?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			var sql = $"{SelectSql} WHERE normalized_name = @name;";
			var command = new CommandDefinition(sql, new { name }, cancellationToken: cancellationToken);
			return DbConnection.QuerySingleOrDefaultAsync<TNamedEntity?>(command);
		}
	}
}

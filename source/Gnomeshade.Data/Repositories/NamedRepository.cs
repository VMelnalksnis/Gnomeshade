// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

using static Gnomeshade.Data.Repositories.AccessLevel;

namespace Gnomeshade.Data.Repositories;

/// <summary>A base class for <see cref="INamedEntity"/> implementing queries based on entity name.</summary>
/// <typeparam name="TNamedEntity">The type of entity that will be queried with this repository.</typeparam>
public abstract class NamedRepository<TNamedEntity> : Repository<TNamedEntity>
	where TNamedEntity : class, INamedEntity
{
	/// <summary>Initializes a new instance of the <see cref="NamedRepository{TNamedEntity}"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	protected NamedRepository(ILogger<NamedRepository<TNamedEntity>> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <summary>Gets SQL where clause that filters for specific entity by name.</summary>
	protected virtual string NameSql => "normalized_name = upper(@name)";

	/// <summary>Finds an entity by its normalized name.</summary>
	/// <param name="name">The normalized name of the entity to find.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TNamedEntity?> FindByNameAsync(string name, Guid userId, CancellationToken cancellationToken = default)
	{
		Logger.FindName(name);
		return FindAsync(new(
			$"{SelectActiveSql} AND {NameSql};",
			new { name, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Finds an entity by its normalized name.</summary>
	/// <param name="name">The normalized name of the entity to find.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TNamedEntity?> FindByNameAsync(
		string name,
		Guid userId,
		DbTransaction dbTransaction,
		CancellationToken cancellationToken = default)
	{
		Logger.FindNameWithTransaction(name);
		return FindAsync(new(
			$"{SelectActiveSql} AND {NameSql};",
			new { name, userId, access = Read.ToParam() },
			dbTransaction,
			cancellationToken: cancellationToken));
	}
}

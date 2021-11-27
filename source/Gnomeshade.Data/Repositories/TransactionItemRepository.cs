// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="TransactionItemEntity"/> repository.
/// </summary>
public sealed class TransactionItemRepository : Repository<TransactionItemEntity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionItemRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransactionItemRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.TransactionItem.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.TransactionItem.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.TransactionItem.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.TransactionItem.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE ti.id = @id AND ownerships.user_id = @ownerId";
}

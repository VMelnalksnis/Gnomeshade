// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="AccountEntity"/> repository.</summary>
public sealed class Loan2Repository : NamedRepository<Loan2Entity>
{
	/// <summary>Initializes a new instance of the <see cref="Loan2Repository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public Loan2Repository(ILogger<Loan2Repository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Loan2.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Loan2.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Loan2.SelectAll;

	/// <inheritdoc />
	protected override string FindSql => "loans2.id = @id";

	protected override string GroupBy => string.Empty;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Loan2.Update;

	/// <inheritdoc />
	protected override string NotDeleted => "loans2.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "loans2.normalized_name = upper(@name)";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Loan2.Select;
}

﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="CounterpartyEntity"/> repository.
/// </summary>
public sealed class CounterpartyRepository : Repository<CounterpartyEntity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CounterpartyRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public CounterpartyRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Counterparty.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Counterparty.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Counterparty.Select;

	/// <inheritdoc />
	protected override string UpdateSql => throw new NotImplementedException();

	/// <inheritdoc />
	protected override string FindSql => $"WHERE c.id = @id AND ownerships.user_id = @ownerId {_accessSql}";
}

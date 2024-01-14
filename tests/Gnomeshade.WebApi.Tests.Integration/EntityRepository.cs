// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.WebApi.Tests.Integration;

internal sealed class EntityRepository
{
	private static readonly Dictionary<Type, string> _tableNames = new()
	{
		{ typeof(AccessEntity), "accesses" },
		{ typeof(AccountEntity), "accounts" },
		{ typeof(AccountInCurrencyEntity), "account_in_currencies" },
		{ typeof(CategoryEntity), "categories" },
		{ typeof(CounterpartyEntity), "counterparties" },
		{ typeof(CurrencyEntity), "currencies" },
		{ typeof(LinkEntity), "links" },
		{ typeof(LoanEntity), "loans" },
		{ typeof(OwnerEntity), "owners" },
		{ typeof(OwnershipEntity), "ownerships" },
		{ typeof(ProductEntity), "products" },
		{ typeof(PurchaseEntity), "purchases" },
		{ typeof(TransactionEntity), "transactions" },
		{ typeof(TransferEntity), "transfers" },
		{ typeof(UnitEntity), "units" },
		{ typeof(UserEntity), "users" },
	};

	private readonly DbConnection _dbConnection;

	public EntityRepository(DbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	internal async Task<IEntity?> FindByIdAsync<TEntity>(Guid id)
		where TEntity : Entity
	{
		var tableName = _tableNames[typeof(TEntity)];
		var sql = @$"SELECT 
    id AS Id, created_at CreatedAt, created_by_user_id CreatedByUserId, deleted_at DeletedAt, deleted_by_user_id DeletedByUserId
FROM {tableName}
WHERE {tableName}.id = @id;";
		return await _dbConnection.QuerySingleOrDefaultAsync<AnyEntity>(new(sql, new { id }));
	}

	private sealed record AnyEntity : Entity;
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class AccountRepository : IRepository<Account>, IDisposable
	{
		private const string _insertSql = "INSERT INTO accounts (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, preferred_currency_id, bic, iban, account_number) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedNamed, @PreferredCurrency, @Bic, @Iban, @AccountNumber) RETURNING id";
		private const string _selectSql = "SELECT id, created_at CreatedAt, owner_id OwnerId, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, name, normalized_name NormalizedName, preferred_currency_id PreferredCurrencyId, bic, iban, account_number AccountNumber FROM accounts";
		private const string _deleteSql = "DELETE FROM accounts WHERE id = @Id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public AccountRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(Account entity) => await _dbConnection.QuerySingleAsync<Guid>(_insertSql);

		/// <inheritdoc />
		public async Task<Guid> AddAsync(Account entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command);
		}

		/// <inheritdoc />
		public async Task<Account?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleOrDefaultAsync<Account>(command);
		}

		/// <inheritdoc />
		public async Task<Account> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleAsync<Account>(command);
		}

		/// <inheritdoc />
		public async Task<int> DeleteAsync(Guid id) => await _dbConnection.ExecuteAsync(_deleteSql, new { id });

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();
	}
}

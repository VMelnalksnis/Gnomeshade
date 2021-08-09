// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data
{
	/// <summary>
	/// Performs bulk actions on account entities.
	/// </summary>
	public sealed class AccountUnitOfWork : IDisposable
	{
		private readonly IDbConnection _dbConnection;
		private readonly AccountRepository _repository;
		private readonly AccountInCurrencyRepository _inCurrencyRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountUnitOfWork"/> class.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		/// <param name="repository">The repository for managing accounts.</param>
		/// <param name="inCurrencyRepository">The repository for managing accounts in currencies.</param>
		public AccountUnitOfWork(
			IDbConnection dbConnection,
			AccountRepository repository,
			AccountInCurrencyRepository inCurrencyRepository)
		{
			_dbConnection = dbConnection;
			_repository = repository;
			_inCurrencyRepository = inCurrencyRepository;
		}

		/// <summary>
		/// Creates a new account with the currencies in <see cref="Account.Currencies"/>.
		/// </summary>
		/// <param name="account">The account to create.</param>
		/// <returns>The id of the created account.</returns>
		public async Task<Guid> AddAsync(Account account)
		{
			using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

			try
			{
				var id = await AddAsync(account, dbTransaction).ConfigureAwait(false);
				dbTransaction.Commit();
				return id;
			}
			catch (Exception)
			{
				dbTransaction.Rollback();
				throw;
			}
		}

		/// <summary>
		/// Creates a new account with the currencies in <see cref="Account.Currencies"/>.
		/// </summary>
		/// <param name="account">The account to create.</param>
		/// <param name="dbTransaction">The database transaction to use for queries.</param>
		/// <returns>The id of the created account.</returns>
		public async Task<Guid> AddAsync(Account account, IDbTransaction dbTransaction)
		{
			var id = await _repository.AddAsync(account, dbTransaction).ConfigureAwait(false);

			foreach (var currencyId in account.Currencies.Select(inCurrency => inCurrency.CurrencyId))
			{
				var inCurrency = new AccountInCurrency
				{
					OwnerId = account.OwnerId,
					CreatedByUserId = account.CreatedByUserId,
					ModifiedByUserId = account.ModifiedByUserId,
					AccountId = id,
					CurrencyId = currencyId,
				};

				_ = await _inCurrencyRepository.AddAsync(inCurrency, dbTransaction);
			}

			return id;
		}

		/// <summary>
		/// Deletes the specified account and all its currencies.
		/// </summary>
		/// <param name="account">The account to delete.</param>
		/// <returns>The number of affected rows.</returns>
		public async Task<int> DeleteAsync(Account account)
		{
			using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

			try
			{
				var rows = await DeleteAsync(account, dbTransaction).ConfigureAwait(false);
				dbTransaction.Commit();
				return rows;
			}
			catch (Exception)
			{
				dbTransaction.Rollback();
				throw;
			}
		}

		/// <summary>
		/// Deletes the specified account and all its currencies using the specified transaction.
		/// </summary>
		/// <param name="account">The account to delete.</param>
		/// <param name="dbTransaction">The database transaction to use for the queries.</param>
		/// <returns>The number of affected rows.</returns>
		public async Task<int> DeleteAsync(Account account, IDbTransaction dbTransaction)
		{
			var rows = 0;
			foreach (var currency in account.Currencies)
			{
				rows += await _inCurrencyRepository.DeleteAsync(currency.Id, dbTransaction).ConfigureAwait(false);
			}

			rows += await _repository.DeleteAsync(account.Id, dbTransaction).ConfigureAwait(false);
			return rows;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_inCurrencyRepository.Dispose();
		}
	}
}

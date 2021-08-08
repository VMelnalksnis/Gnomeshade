// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data
{
	public sealed class AccountUnitOfWork : IDisposable
	{
		private readonly IDbConnection _dbConnection;
		private readonly AccountRepository _repository;
		private readonly AccountInCurrencyRepository _inCurrencyRepository;

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
		/// Adds a new account inferring the currency from <see cref="Account.PreferredCurrencyId"/>.
		/// </summary>
		/// <param name="account">The account to create.</param>
		/// <returns>The id of the created account.</returns>
		public Task<Guid> AddAsync(Account account) => AddAsync(account, account.PreferredCurrencyId);

		/// <summary>
		/// Adds a new account with the specified currency.
		/// </summary>
		/// <param name="account">The account to create.</param>
		/// <param name="currency">The single currency to add to the account.</param>
		/// <returns>The id of the created account.</returns>
		public Task<Guid> AddAsync(Account account, Currency currency) => AddAsync(account, currency.Id);

		/// <summary>
		/// Adds a new account with a single currency with the specified id.
		/// </summary>
		/// <param name="account">The account to create.</param>
		/// <param name="currencyId">The id of the single currency to add to the account.</param>
		/// <returns>The id of the created account.</returns>
		public Task<Guid> AddAsync(Account account, Guid currencyId)
		{
			return AddAsync(account, currencyId, new[] { currencyId });
		}

		/// <summary>
		/// Adds a new account with the specified preferred currency and additional currencies.
		/// </summary>
		/// <param name="account">The account to create.</param>
		/// <param name="currencyId">The id of the preferred currency.</param>
		/// <param name="currencies">The ids of the additional currencies to add.</param>
		/// <returns>The id of the created account.</returns>
		public async Task<Guid> AddAsync(Account account, Guid currencyId, IReadOnlyCollection<Guid> currencies)
		{
			using var dbTransaction = _dbConnection.OpenAndBeginTransaction();
			try
			{
				account.PreferredCurrencyId = currencyId;
				var id = await _repository.AddAsync(account, dbTransaction).ConfigureAwait(false);

				var currenciesToAdd = currencies.Contains(currencyId) ? currencies : currencies.Append(currencyId);
				foreach (var currency in currenciesToAdd)
				{
					var inCurrency = new AccountInCurrency
					{
						OwnerId = account.OwnerId,
						CreatedByUserId = account.CreatedByUserId,
						ModifiedByUserId = account.ModifiedByUserId,
						AccountId = id,
						CurrencyId = currency,
					};

					_ = await _inCurrencyRepository.AddAsync(inCurrency, dbTransaction).ConfigureAwait(false);
				}

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
		/// Deletes the specified account and all its currencies.
		/// </summary>
		/// <param name="account">The account to delete.</param>
		/// <returns>The count of affected entities.</returns>
		public async Task<int> DeleteAsync(Account account)
		{
			using var dbTransaction = _dbConnection.OpenAndBeginTransaction();
			try
			{
				var affectedEntities = 0;
				foreach (var currency in account.Currencies)
				{
					affectedEntities += await _inCurrencyRepository.DeleteAsync(currency.Id, dbTransaction).ConfigureAwait(false);
				}

				affectedEntities += await _repository.DeleteAsync(account.Id, dbTransaction).ConfigureAwait(false);
				dbTransaction.Commit();
				return affectedEntities;
			}
			catch (Exception)
			{
				dbTransaction.Rollback();
				throw;
			}
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

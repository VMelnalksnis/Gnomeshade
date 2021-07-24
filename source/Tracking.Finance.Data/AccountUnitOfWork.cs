// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

namespace Tracking.Finance.Data
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
		/// <param name="currencyId">The if of the single currency to add to the account.</param>
		/// <returns>The id of the created account.</returns>
		public async Task<Guid> AddAsync(Account account, Guid currencyId)
		{
			if (!_dbConnection.State.HasFlag(ConnectionState.Open))
			{
				_dbConnection.Open();
			}

			using var dbTransaction = _dbConnection.BeginTransaction();
			try
			{
				account.PreferredCurrencyId = currencyId;
				var id = await _repository.AddAsync(account, dbTransaction).ConfigureAwait(false);

				var inCurrency = new AccountInCurrency
				{
					OwnerId = account.OwnerId,
					CreatedByUserId = account.CreatedByUserId,
					ModifiedByUserId = account.ModifiedByUserId,
					AccountId = id,
					CurrencyId = currencyId,
				};

				_ = await _inCurrencyRepository.AddAsync(inCurrency, dbTransaction).ConfigureAwait(false);
				dbTransaction.Commit();

				return id;
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

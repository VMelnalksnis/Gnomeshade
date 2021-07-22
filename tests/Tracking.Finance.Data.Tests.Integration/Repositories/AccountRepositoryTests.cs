// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Data.TestingHelpers;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class AccountRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private AccountRepository _repository = null!;
		private AccountInCurrencyRepository _inCurrencyRepository = null!;
		private List<Currency> _currencies = null!;
		private Guid _currencyId;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
			_inCurrencyRepository = new(_dbConnection);

			var currencyRepository = new CurrencyRepository(_dbConnection);
			_currencies = await currencyRepository.GetAllAsync().ConfigureAwait(false);
			_currencyId = _currencies.First().Id;
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
		}

		[Test]
		public async Task AddAsync_WithTransaction()
		{
			using var dbTransaction = _dbConnection.BeginTransaction();
			var account = new AccountFaker(TestUser.Id, _currencyId).Generate();
			var accountId = await _repository.AddAsync(account, dbTransaction);
			var inCurrencies =
				_currencies
					.Select(currency => new AccountInCurrencyFaker(TestUser.Id, accountId, currency.Id).Generate())
					.Select(inCurrency =>
					{
						var id = _inCurrencyRepository.AddAsync(inCurrency, dbTransaction).GetAwaiter().GetResult();
						return inCurrency with { Id = id };
					})
					.ToList();

			dbTransaction.Commit();

			var getAccount = await _repository.GetByIdAsync(accountId);
			var findAccount = await _repository.FindByIdAsync(getAccount.Id);
			var findByNameAccount = await _repository.FindByNameAsync(getAccount.NormalizedName);
			var accounts = await _repository.GetAllAsync();

			var expectedAccount = account with
			{
				Id = accountId,
				CreatedAt = getAccount.CreatedAt,
				ModifiedAt = getAccount.ModifiedAt,
				PreferredCurrency = getAccount.PreferredCurrency,
				Currencies = getAccount.Currencies,
			};

			expectedAccount.Currencies.Should().NotBeNullOrEmpty();

			getAccount.Should().BeEquivalentTo(expectedAccount);
			findAccount.Should().BeEquivalentTo(expectedAccount);
			findByNameAccount.Should().BeEquivalentTo(expectedAccount);
			accounts.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedAccount);

			var firstAccountInCurrency = inCurrencies.First();
			var getAccountInCurrency = await _inCurrencyRepository.GetByIdAsync(firstAccountInCurrency.Id);
			var findAccountInCurrency = await _inCurrencyRepository.FindByIdAsync(getAccountInCurrency.Id);

			var expectedAccountInCurrency = firstAccountInCurrency with
			{
				CreatedAt = getAccountInCurrency.CreatedAt,
				ModifiedAt = getAccountInCurrency.ModifiedAt,
				Currency = getAccountInCurrency.Currency,
			};

			getAccountInCurrency.Should().BeEquivalentTo(expectedAccountInCurrency);
			findAccountInCurrency.Should().BeEquivalentTo(expectedAccountInCurrency);

			foreach (var inCurrency in inCurrencies)
			{
				await _inCurrencyRepository.DeleteAsync(inCurrency.Id);
			}

			await _repository.DeleteAsync(accountId);
		}
	}
}

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
		private AccountUnitOfWork _unitOfWork = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
			_inCurrencyRepository = new(_dbConnection);
			_unitOfWork = new(_dbConnection, _repository, _inCurrencyRepository);
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
			var currencies = await EntityFactory.GetCurrenciesAsync();
			var preferredCurrency = currencies.First();

			var account = new AccountFaker(TestUser, preferredCurrency).Generate();

			var currencyIds =
				currencies
					.Select(currency => currency.Id)
					.ToList();

			var accountId = await _unitOfWork.AddAsync(account, account.PreferredCurrencyId, currencyIds);

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

			var firstAccountInCurrency = getAccount.Currencies.First();
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

			await _unitOfWork.DeleteAsync(getAccount);
		}
	}
}

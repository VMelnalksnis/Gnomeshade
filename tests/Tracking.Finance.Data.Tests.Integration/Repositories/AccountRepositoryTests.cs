// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;
using static Tracking.Finance.Data.Tests.Integration.Repositories.FluentAssertionsConfiguration;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class AccountRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private AccountRepository _repository = null!;
		private AccountInCurrencyRepository _inCurrencyRepository = null!;
		private Guid _ownerId;
		private Currency _currency = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
			_inCurrencyRepository = new(_dbConnection);

			_ownerId = TestUser.Id;

			var currencyRepository = new CurrencyRepository(_dbConnection);
			_currency = (await currencyRepository.GetAllAsync().ConfigureAwait(false)).First();
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
		}

		[Test]
		public async Task AddAsync()
		{
			using var dbTransaction = _dbConnection.BeginTransaction();

			var account = new Account
			{
				OwnerId = _ownerId,
				CreatedByUserId = _ownerId,
				ModifiedByUserId = _ownerId,
				PreferredCurrencyId = _currency.Id,
			};

			var id = await _repository.AddAsync(account, dbTransaction);

			var accountInCurrency = new AccountInCurrency
			{
				OwnerId = _ownerId,
				CreatedByUserId = _ownerId,
				ModifiedByUserId = _ownerId,
				AccountId = id,
				CurrencyId = _currency.Id,
			};

			var inCurrencyId = await _inCurrencyRepository.AddAsync(accountInCurrency, dbTransaction);

			dbTransaction.Commit();

			var inCurrency = await _inCurrencyRepository.FindByIdAsync(inCurrencyId);
			inCurrency.Should().BeEquivalentTo(accountInCurrency, ModifiableWithoutIdOptions);
		}
	}
}

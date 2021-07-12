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
using Tracking.Finance.Data.TestingHelpers;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class TransactionItemRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _transactionRepository = null!;
		private TransactionItemRepository _repository = null!;
		private TransactionItem _transactionItem = null!;

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_transactionRepository = new(_dbConnection);
			_repository = new(_dbConnection);

			var currency = (await new CurrencyRepository(_dbConnection).GetAllAsync().ConfigureAwait(false)).First();
			var product = new ProductFaker(TestUser).Generate();
			var productId = await new ProductRepository(_dbConnection).AddAsync(product).ConfigureAwait(false);

			var accountFaker = new AccountFaker(TestUser, currency);
			var sourceAccount = accountFaker.Generate();
			var targetAccount = accountFaker.GenerateUnique(sourceAccount);

			var accountRepository = new AccountRepository(_dbConnection);
			var accountInCurrencyRepository = new AccountInCurrencyRepository(_dbConnection);

			var sourceAccountId = await accountRepository.AddAsync(sourceAccount).ConfigureAwait(false);
			var targetAccountId = await accountRepository.AddAsync(targetAccount).ConfigureAwait(false);

			var sourceInCurrency = new AccountInCurrency
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				AccountId = sourceAccountId,
				CurrencyId = currency.Id,
			};

			var targetInCurrency = new AccountInCurrency
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				AccountId = targetAccountId,
				CurrencyId = currency.Id,
			};

			var sourceInCurrencyId = await accountInCurrencyRepository.AddAsync(sourceInCurrency).ConfigureAwait(false);
			var targetInCurrencyId = await accountInCurrencyRepository.AddAsync(targetInCurrency).ConfigureAwait(false);

			var transaction = new Transaction
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
			};

			var transactionId = await new TransactionRepository(_dbConnection).AddAsync(transaction).ConfigureAwait(false);

			_transactionItem = new()
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				TransactionId = transactionId,
				SourceAccountId = sourceInCurrencyId,
				TargetAccountId = targetInCurrencyId,
				ProductId = productId,
				SourceAmount = 123.45m,
				TargetAmount = 123.45m,
				Amount = 2,
			};
		}

		[OneTimeTearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_transactionRepository.Dispose();
		}

		[Test]
		public async Task AddGetDelete_WithoutTransaction()
		{
			var item = _transactionItem with { };

			var itemId = await _repository.AddAsync(item);

			var getItem = await _repository.GetByIdAsync(itemId);
			var findItem = await _repository.FindByIdAsync(getItem.Id);
			var expectedItem = item with
			{
				Id = itemId,
				CreatedAt = getItem.CreatedAt,
				ModifiedAt = getItem.ModifiedAt,
			};

			getItem.Should().BeEquivalentTo(expectedItem);
			findItem.Should().BeEquivalentTo(expectedItem);

			await _repository.DeleteAsync(itemId);

			var findAfterDelete = await _repository.FindByIdAsync(itemId);
			findAfterDelete.Should().BeNull();
		}

		[Test]
		public async Task AddGetDelete_WithTransaction()
		{
			var item = _transactionItem with { };

			using var dbTransaction = _dbConnection.BeginTransaction();
			var itemId = await _repository.AddAsync(item, dbTransaction);
			dbTransaction.Commit();

			var getItem = await _repository.GetByIdAsync(itemId);
			var findItem = await _repository.FindByIdAsync(getItem.Id);
			var expectedItem = item with
			{
				Id = itemId,
				CreatedAt = getItem.CreatedAt,
				ModifiedAt = getItem.ModifiedAt,
			};

			getItem.Should().BeEquivalentTo(expectedItem);
			findItem.Should().BeEquivalentTo(expectedItem);

			await _repository.DeleteAsync(itemId);

			var findAfterDelete = await _repository.FindByIdAsync(itemId);
			findAfterDelete.Should().BeNull();
		}
	}
}

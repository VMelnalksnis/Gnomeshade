// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Npgsql;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class TransactionItemRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _transactionRepository = null!;
		private TransactionItemRepository _repository = null!;
		private Transaction _defaultTransaction = null!;
		private TransactionItem _defaultItem = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_transactionRepository = new(_dbConnection);
			_repository = new(_dbConnection);

			_defaultTransaction = new()
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
			};

			using var transaction = _dbConnection.BeginTransaction();
			var currency = (await new CurrencyRepository(_dbConnection).GetAllAsync().ConfigureAwait(false)).First();
			var accountRepository = new AccountRepository(_dbConnection);
			var inCurrencyRepository = new AccountInCurrencyRepository(_dbConnection);
			var account = new Account
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				PreferredCurrencyId = currency.Id,
			};

			var accountId = await accountRepository.AddAsync(account, transaction).ConfigureAwait(false);
			var inCurrency = new AccountInCurrency
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				AccountId = accountId,
				CurrencyId = currency.Id,
			};
			var inCurrencyId = await inCurrencyRepository.AddAsync(inCurrency, transaction).ConfigureAwait(false);
			transaction.Commit();

			_defaultItem = new()
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				SourceAccountId = inCurrencyId,
				TargetAccountId = inCurrencyId,
			};
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_transactionRepository.Dispose();
		}

		[Test]
		public void AddAsync_ShouldThrowOnInvalidForeignKey()
		{
			var transactionItem = _defaultItem with { };

			FluentActions
				.Awaiting(() => _repository.AddAsync(transactionItem))
				.Should()
				.ThrowExactly<PostgresException>()
				.Which.ConstraintName.Should()
				.Be("transaction_items_transaction_id_fkey");
		}

		[Test]
		public async Task AddAsync_ShouldBeEquivalent()
		{
			var transactionId = await _transactionRepository.AddAsync(_defaultTransaction with { });
			var transactionItem = _defaultItem with { TransactionId = transactionId };

			var id = await _repository.AddAsync(transactionItem);
			transactionItem = transactionItem with { Id = id };
			var actual = await _repository.GetByIdAsync(id);

			actual.Should().BeEquivalentTo(transactionItem, FluentAssertionsConfiguration.ModifiableOptions);
			await _repository.DeleteAsync(id);
		}

		[Test]
		public async Task GetAllAsync_ShouldReturnExpected()
		{
			var transactionId = await _transactionRepository.AddAsync(_defaultTransaction with { });
			var id1 = await _repository.AddAsync(_defaultItem with { TransactionId = transactionId });
			var id2 = await _repository.AddAsync(_defaultItem with { TransactionId = transactionId });
			var id3 = await _repository.AddAsync(_defaultItem with { TransactionId = transactionId });

			var items = await _repository.GetAllAsync(transactionId);

			items.Should().HaveCount(3);
			items.Should().AllBeEquivalentTo(
				_defaultItem with { TransactionId = transactionId },
				FluentAssertionsConfiguration.ModifiableWithoutIdOptions);

			await _repository.DeleteAsync(id1);
			await _repository.DeleteAsync(id2);
			await _repository.DeleteAsync(id3);
		}
	}
}

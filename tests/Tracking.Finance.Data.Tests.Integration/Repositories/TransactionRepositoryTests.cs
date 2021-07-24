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
	public class TransactionRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _repository = null!;
		private TransactionItemRepository _itemRepository = null!;
		private Transaction _defaultTransaction = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
			_itemRepository = new(_dbConnection);

			_defaultTransaction = new()
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
			};
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_itemRepository.Dispose();
		}

		[Test]
		public async Task AddGetDelete_WithoutTransaction()
		{
			var product = await EntityFactory.GetProductAsync();

			var transactionToAdd = new TransactionFaker(TestUser).Generate();
			var importHash = await transactionToAdd.GetHashAsync();
			transactionToAdd = transactionToAdd with { ImportHash = importHash };
			var transactionId = await _repository.AddAsync(transactionToAdd);
			transactionToAdd = transactionToAdd with { Id = transactionId };

			var (first, second) = await EntityFactory.GetAccountsAsync();
			var transactionItemToAdd = new TransactionItemFaker(
					TestUser,
					transactionToAdd,
					first.Currencies.Single(),
					second.Currencies.Single(),
					product)
				.Generate();
			var itemId = await _itemRepository.AddAsync(transactionItemToAdd);

			var getTransaction = await _repository.GetByIdAsync(transactionId);
			var findTransaction = await _repository.FindByIdAsync(getTransaction.Id);
			var findImportTransaction = await _repository.FindByImportHashAsync(importHash);
			var allTransactions = await _repository.GetAllAsync();

			getTransaction.Items.Should().ContainSingle().Which.Product.Should().NotBeNull();
			findTransaction.Should().BeEquivalentTo(getTransaction, options => options.ComparingByMembers<Transaction>().ComparingByMembers<TransactionItem>());
			findImportTransaction.Should().BeEquivalentTo(getTransaction, options => options.ComparingByMembers<Transaction>().ComparingByMembers<TransactionItem>());
			allTransactions.Should().ContainSingle().Which.Should().BeEquivalentTo(getTransaction, options => options.ComparingByMembers<Transaction>().ComparingByMembers<TransactionItem>());

			await _itemRepository.DeleteAsync(itemId);
			await _repository.DeleteAsync(transactionId);
			await new ProductRepository(_dbConnection).DeleteAsync(product.Id);
			await new AccountInCurrencyRepository(_dbConnection).DeleteAsync(first.Currencies.Single().Id);
			await new AccountInCurrencyRepository(_dbConnection).DeleteAsync(second.Currencies.Single().Id);
			await new AccountRepository(_dbConnection).DeleteAsync(first.Id);
			await new AccountRepository(_dbConnection).DeleteAsync(second.Id);
		}

		[Test]
		public async Task DeleteAsync_ShouldDelete()
		{
			var id = await _repository.AddAsync(_defaultTransaction with { });

			var result = await _repository.DeleteAsync(id);

			result.Should().Be(1);
		}

		[Test]
		public async Task FindByIdAsync_ShouldReturnNullIfDoesNotExist()
		{
			(await _repository.FindByIdAsync(Guid.NewGuid())).Should().BeNull();
		}

		[Test]
		public async Task FindByImportHashAsync_ShouldReturnNullIfDoesNotExist()
		{
			var importHash = await new Transaction().GetHashAsync();

			var transaction = await _repository.FindByImportHashAsync(importHash);

			transaction.Should().BeNull();
		}
	}
}

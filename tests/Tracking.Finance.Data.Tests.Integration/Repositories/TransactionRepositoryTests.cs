// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;
using static Tracking.Finance.Data.Tests.Integration.Repositories.FluentAssertionsConfiguration;

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
		public async Task GetAllAsync_ShouldReturnExpected()
		{
			var existingTransactions = await _repository.GetAllAsync();
			foreach (var existingTransaction in existingTransactions)
			{
				var items = await _itemRepository.GetAllAsync(existingTransaction.Id);
				foreach (var item in items)
				{
					await _itemRepository.DeleteAsync(item.Id);
				}

				await _repository.DeleteAsync(existingTransaction.Id);
			}

			await _repository.AddAsync(_defaultTransaction with { });
			await _repository.AddAsync(_defaultTransaction with { });
			await _repository.AddAsync(_defaultTransaction with { });

			var transactions = await _repository.GetAllAsync();

			transactions.Should().HaveCount(3);
			transactions.Should().AllBeEquivalentTo(_defaultTransaction, ModifiableWithoutIdOptions);
		}

		[Test]
		public async Task AddAsync_ShouldBeEquivalent()
		{
			var transaction = _defaultTransaction with { };

			var id = await _repository.AddAsync(transaction);
			transaction = transaction with { Id = id };

			var actual = await _repository.GetByIdAsync(id);
			actual.Should().BeEquivalentTo(transaction, ModifiableOptions);
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
			var transaction = await _repository.FindByIdAsync(Guid.NewGuid());

			transaction.Should().BeNull();
		}

		[Test]
		public async Task FindByIdAsync_ShouldReturnExpectedIfExists()
		{
			var transaction = _defaultTransaction with { };

			var id = await _repository.AddAsync(transaction);
			transaction = transaction with { Id = id };

			var actual = await _repository.FindByIdAsync(id);
			actual.Should().BeEquivalentTo(transaction, ModifiableOptions);
		}
	}
}

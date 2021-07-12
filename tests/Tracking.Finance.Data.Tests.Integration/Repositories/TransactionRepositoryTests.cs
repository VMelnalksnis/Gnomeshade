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

			var transactionsToAdd = new List<Transaction>
			{
				_defaultTransaction with { },
				_defaultTransaction with { },
				_defaultTransaction with { },
			};

			foreach (var transactionToAdd in transactionsToAdd)
			{
				await _repository.AddAsync(transactionToAdd);
			}

			var actualTransactions = await _repository.GetAllAsync();
			var expectedTransactions =
				transactionsToAdd.Zip(actualTransactions)
					.Select(tuple => tuple.First with
					{
						Id = tuple.Second.Id,
						CreatedAt = tuple.Second.CreatedAt,
						ModifiedAt = tuple.Second.ModifiedAt,
					}).ToList();

			actualTransactions.Should().HaveCount(transactionsToAdd.Count);
			actualTransactions.Should().BeEquivalentTo(expectedTransactions);
		}

		[Test]
		public async Task AddAsync_ShouldBeEquivalent()
		{
			var transactionToAdd = _defaultTransaction with { };

			var transactionId = await _repository.AddAsync(transactionToAdd);

			var getTransaction = await _repository.GetByIdAsync(transactionId);
			var expectedTransaction = getTransaction with
			{
				Id = transactionId,
				CreatedAt = getTransaction.CreatedAt,
				ModifiedAt = getTransaction.ModifiedAt,
			};

			getTransaction.Should().BeEquivalentTo(expectedTransaction);
			await _repository.DeleteAsync(transactionId);
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
		public async Task FindByIdAsync_ShouldReturnExpectedIfExists()
		{
			var transactionToAdd = _defaultTransaction with { };
			var transactionId = await _repository.AddAsync(transactionToAdd);

			var findTransaction = await _repository.FindByIdAsync(transactionId);
			findTransaction.Should().NotBeNull();

			var expectedTransaction = findTransaction! with
			{
				Id = transactionId,
				CreatedAt = findTransaction.CreatedAt,
				ModifiedAt = findTransaction.ModifiedAt,
			};

			findTransaction.Should().BeEquivalentTo(expectedTransaction);
		}
	}
}

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

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class TransactionRepositoryTests
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _repository = null!;
		private TransactionItemRepository _itemRepository = null!;

		[SetUp]
		public async Task SetUp()
		{
			_dbConnection = await DatabaseInitialization.CreateConnection();
			_repository = new TransactionRepository(_dbConnection);
			_itemRepository = new TransactionItemRepository(_dbConnection);
		}

		[Test]
		public async Task GetAllAsync_ShouldReturnExpected()
		{
			var existingTransactions = await _repository.GetAllAsync();
			foreach (var transaction in existingTransactions)
			{
				var items = await _itemRepository.GetAllAsync(transaction.Id);
				foreach (var item in items)
				{
					await _itemRepository.DeleteAsync(item.Id);
				}

				await _repository.DeleteAsync(transaction.Id);
			}

			await _repository.AddAsync(new Transaction());
			await _repository.AddAsync(new Transaction());
			await _repository.AddAsync(new Transaction());

			var transactions = await _repository.GetAllAsync();

			transactions.Should().HaveCount(3);
			transactions
				.Should()
				.AllBeEquivalentTo(
					new Transaction(),
					options => FluentAssertionsConfiguration.ModifiableOptions(options).Excluding(transaction => transaction.Id));
		}

		[Test]
		public async Task AddAsync_ShouldBeEquivalent()
		{
			var transaction = new Transaction();

			var id = await _repository.AddAsync(transaction);
			transaction = transaction with { Id = id };

			var actual = await _repository.GetByIdAsync(id);
			actual.Should().BeEquivalentTo(transaction, FluentAssertionsConfiguration.ModifiableOptions);
		}

		[Test]
		public async Task DeleteAsync_ShouldDelete()
		{
			var id = await _repository.AddAsync(new Transaction());

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
			var transaction = new Transaction();

			var id = await _repository.AddAsync(transaction);
			transaction = transaction with { Id = id };

			var actual = await _repository.FindByIdAsync(id);
			actual.Should().BeEquivalentTo(transaction, options => options.ComparingByMembers<Transaction>().Excluding(t => t.CreatedAt).Excluding(t => t.ModifiedAt));
		}

		[TearDown]
		public void TearDown()
		{
			_dbConnection.Dispose();
		}
	}
}

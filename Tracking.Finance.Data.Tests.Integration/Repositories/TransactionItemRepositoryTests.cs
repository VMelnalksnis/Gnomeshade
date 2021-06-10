// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using Npgsql;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class TransactionItemRepositoryTests
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _transactionRepository = null!;
		private TransactionItemRepository _repository = null!;

		[SetUp]
		public async Task SetUp()
		{
			_dbConnection = await DatabaseInitialization.CreateConnection();
			_transactionRepository = new(_dbConnection);
			_repository = new(_dbConnection);
		}

		[Test]
		public void AddAsync_ShouldThrowOnInvalidForeignKey()
		{
			var transactionItem = new TransactionItem();

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
			var transactionId = await _transactionRepository.AddAsync(new());
			var transactionItem = new TransactionItem { TransactionId = transactionId };

			var id = await _repository.AddAsync(transactionItem);
			transactionItem = transactionItem with { Id = id };
			var actual = await _repository.GetByIdAsync(id);

			actual.Should().BeEquivalentTo(transactionItem, FluentAssertionsConfiguration.ModifiableOptions);
			await _repository.DeleteAsync(id);
		}

		[Test]
		public async Task GetAllAsync_ShouldReturnExpected()
		{
			var transactionId = await _transactionRepository.AddAsync(new());
			var id1 = await _repository.AddAsync(new()
				{ TransactionId = transactionId });
			var id2 = await _repository.AddAsync(new()
				{ TransactionId = transactionId });
			var id3 = await _repository.AddAsync(new()
				{ TransactionId = transactionId });

			var items = await _repository.GetAllAsync(transactionId);

			items.Should().HaveCount(3);
			items.Should().AllBeEquivalentTo(
				new TransactionItem { TransactionId = transactionId },
				FluentAssertionsConfiguration.ModifiableWithoutIdOptions);

			await _repository.DeleteAsync(id1);
			await _repository.DeleteAsync(id2);
			await _repository.DeleteAsync(id3);
		}

		[TearDown]
		public void TearDown()
		{
			_dbConnection.Dispose();
		}
	}
}

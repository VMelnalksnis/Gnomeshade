using FluentAssertions;

using NUnit.Framework;

using System.Data;
using System.Threading.Tasks;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class TransactionRepositoryTests
	{
		private IDbConnection _dbConnection;
		private TransactionRepository _repository;

		[SetUp]
		public async Task SetUp()
		{
			_dbConnection = await DatabaseInitialization.CreateConnection();
			_repository = new TransactionRepository(_dbConnection);
		}

		[Test]
		public async Task GetAllAsync_ShouldReturnExpected()
		{
			var existingTransactions = await _repository.GetAllAsync();
			foreach (var transaction in existingTransactions)
			{
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
					options => options.Excluding(transaction => transaction.Id));
		}

		[Test]
		public async Task AddAsync_ShouldBeEquivalent()
		{
			var transaction = new Transaction();

			var id = await _repository.AddAsync(transaction);
			transaction.Id = id;

			var actual = await _repository.GetByIdAsync(id);
			actual.Should().BeEquivalentTo(transaction);
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
			var transaction = await _repository.FindByIdAsync(int.MinValue);

			transaction.Should().BeNull();
		}

		[Test]
		public async Task FindByIdAsync_ShouldReturnExpectedIfExists()
		{
			var transaction = new Transaction();

			var id = await _repository.AddAsync(transaction);
			transaction.Id = id;

			var actual = await _repository.FindByIdAsync(id);
			actual.Should().BeEquivalentTo(transaction);
		}

		[TearDown]
		public void TearDown()
		{
			_dbConnection.Dispose();
		}
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.TestingHelpers.Data.Fakers;

using NodaTime;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.UnitsOfWork;

public sealed class TransactionUnitOfWorkTests : IDisposable
{
	private IDbConnection _dbConnection = null!;
	private TransactionRepository _repository = null!;
	private TransactionUnitOfWork _unitOfWork = null!;

	[SetUp]
	public async Task OneTimeSetupAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(_dbConnection);
		_unitOfWork = new(_dbConnection, _repository);
	}

	[TearDown]
	public void Dispose()
	{
		_dbConnection.Dispose();
		_repository.Dispose();
		_unitOfWork.Dispose();
	}

	[Test]
	public async Task AddGetDelete_WithoutTransaction()
	{
		var transactionToAdd = new TransactionFaker(TestUser).Generate();

		var transactionId = await _unitOfWork.AddAsync(transactionToAdd);

		var getTransaction = await _repository.GetByIdAsync(transactionId, TestUser.Id);
		var findTransaction = await _repository.FindByIdAsync(getTransaction.Id, TestUser.Id);
		var dbTransaction = _dbConnection.BeginTransaction();
		dbTransaction.Commit();
		var now = SystemClock.Instance.GetCurrentInstant();
		var allTransactions = await _repository.GetAllAsync(now - Duration.FromDays(31), now, TestUser.Id);

		findTransaction.Should().BeEquivalentTo(getTransaction, Options);
		allTransactions.Should().ContainSingle().Which.Should().BeEquivalentTo(getTransaction, Options);

		await _unitOfWork.DeleteAsync(getTransaction, TestUser.Id);
	}

	private static EquivalencyAssertionOptions<TransactionEntity> Options(
		EquivalencyAssertionOptions<TransactionEntity> options)
	{
		return options.ComparingByMembers<TransactionEntity>();
	}
}

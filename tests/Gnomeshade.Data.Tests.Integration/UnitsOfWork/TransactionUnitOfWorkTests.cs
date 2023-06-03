// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using Microsoft.Extensions.Logging.Abstractions;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.UnitsOfWork;

public sealed class TransactionUnitOfWorkTests
{
	private DbConnection _dbConnection = null!;
	private TransactionRepository _repository = null!;
	private TransactionUnitOfWork _unitOfWork = null!;

	[SetUp]
	public async Task OneTimeSetupAsync()
	{
		_dbConnection = await CreateConnectionAsync();
		_repository = new(NullLogger<TransactionRepository>.Instance, _dbConnection);
		_unitOfWork = new(_dbConnection, _repository);
	}

	[Test]
	public async Task AddGetDelete_WithoutTransaction()
	{
		var transactionToAdd = new TransactionFaker(TestUser).Generate();

		var transactionId = await _unitOfWork.AddAsync(transactionToAdd);

		var getTransaction = await _repository.GetByIdAsync(transactionId, TestUser.Id);
		var findTransaction = await _repository.FindByIdAsync(getTransaction.Id, TestUser.Id);
		await using var dbTransaction = await _dbConnection.BeginTransactionAsync();
		await dbTransaction.CommitAsync();
		var allTransactions = await _repository.GetAsync(TestUser.Id);

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

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Bogus;

using FluentAssertions;

using Gnomeshade.Core;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.TestingHelpers.Data.Fakers;

using NUnit.Framework;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public class TransactionItemRepositoryTests
{
	private IDbConnection _dbConnection = null!;
	private TransactionItemRepository _repository = null!;
	private TransactionRepository _transactionRepository = null!;
	private TransactionUnitOfWork _unitOfWork = null!;
	private TransactionEntity _transaction = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(_dbConnection);
		_transactionRepository = new(_dbConnection);
		_unitOfWork = new(_dbConnection, _transactionRepository, _repository);

		var product = await EntityFactory.GetProductAsync();
		var (first, second) = await EntityFactory.GetAccountsAsync();

		var transactionToAdd = new TransactionFaker(TestUser).Generate();
		var importHash = await transactionToAdd.GetHashAsync();
		transactionToAdd = transactionToAdd with { ImportHash = importHash };

		var itemFaker = new TransactionItemFaker(
			TestUser,
			transactionToAdd,
			first.Currencies.Single(),
			second.Currencies.Single(),
			product);

		var transactionItemsToAdd = itemFaker.GenerateBetween(2, 2);
		transactionToAdd = transactionToAdd with { Items = transactionItemsToAdd };
		var id = await _unitOfWork.AddAsync(transactionToAdd);
		_transaction = await new TransactionRepository(_dbConnection).GetByIdAsync(id, TestUser.Id);
	}

	[Test]
	public async Task TagUntag()
	{
		var tag = new TagFaker(TestUser.Id).Generate();
		var tagId = await new TagRepository(_dbConnection).AddAsync(tag);
		var item = (await _repository.GetAllAsync(TestUser.Id)).First();

		await _repository.TagAsync(item.Id, tagId, TestUser.Id);
		(await _repository.GetTaggedAsync(tagId, TestUser.Id))
			.Should()
			.ContainSingle()
			.Which.Id.Should()
			.Be(item.Id);

		await _repository.UntagAsync(item.Id, tagId, TestUser.Id);
		(await _repository.GetTaggedAsync(tagId, TestUser.Id))
			.Should()
			.BeEmpty();
	}

	[TearDown]
	public async Task TearDown()
	{
		await _unitOfWork.DeleteAsync(_transaction, TestUser.Id);
		_dbConnection.Dispose();
		_repository.Dispose();
	}
}

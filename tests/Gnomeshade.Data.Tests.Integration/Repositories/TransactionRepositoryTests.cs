// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Gnomeshade.Core;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public class TransactionRepositoryTests : IDisposable
{
	private IDbConnection _dbConnection = null!;
	private TransactionRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(_dbConnection);
	}

	[TearDown]
	public void Dispose()
	{
		_dbConnection.Dispose();
		_repository.Dispose();
	}

	[Test]
	public async Task FindByIdAsync_ShouldReturnNullIfDoesNotExist()
	{
		var id = Guid.NewGuid();
		var transaction = await _repository.FindByIdAsync(id, TestUser.Id);
		transaction.Should().BeNull();
	}

	[Test]
	public async Task FindByImportHashAsync_ShouldReturnNullIfDoesNotExist()
	{
		var importHash = await new TransactionEntity().GetHashAsync();
		var dbTransaction = _dbConnection.BeginTransaction();
		var transaction = await _repository.FindByImportHashAsync(importHash, TestUser.Id, dbTransaction);
		dbTransaction.Commit();
		transaction.Should().BeNull();
	}
}

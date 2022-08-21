// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public class TransactionRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private TransactionRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(_dbConnection);
	}

	[Test]
	public async Task FindByIdAsync_ShouldReturnNullIfDoesNotExist()
	{
		var id = Guid.NewGuid();
		var transaction = await _repository.FindByIdAsync(id, TestUser.Id);
		transaction.Should().BeNull();
	}
}

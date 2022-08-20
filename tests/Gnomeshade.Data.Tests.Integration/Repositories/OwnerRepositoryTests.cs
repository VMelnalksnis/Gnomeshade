// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class OwnerRepositoryTests : IAsyncDisposable
{
	private DbConnection _dbConnection = null!;
	private OwnerRepository _ownerRepository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
		_ownerRepository = new(_dbConnection);
	}

	[TearDown]
	public async ValueTask DisposeAsync()
	{
		await _dbConnection.DisposeAsync();
		_ownerRepository.Dispose();
	}

	[Test]
	public async Task AddAsync_ShouldGenerateGuid()
	{
		await using var dbTransaction = await _dbConnection.BeginTransactionAsync();
		var id = await _ownerRepository.AddAsync(Guid.NewGuid(), dbTransaction);
		id.Should().NotBe(Guid.Empty);
	}

	[Test]
	public async Task GetAllAsync()
	{
		var owners = await _ownerRepository.GetAllAsync();
		owners.Should().OnlyHaveUniqueItems();
	}
}

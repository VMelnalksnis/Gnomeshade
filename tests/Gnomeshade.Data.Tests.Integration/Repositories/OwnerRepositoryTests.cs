// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.Logging.Abstractions;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class OwnerRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private OwnerRepository _ownerRepository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
		_ownerRepository = new(NullLogger<OwnerRepository>.Instance, _dbConnection);
	}

	[Test]
	public async Task AddAsync_ShouldGenerateGuid()
	{
		await using var dbTransaction = await _dbConnection.BeginTransactionAsync();

		await _ownerRepository.AddAsync(
			new()
			{
				Id = Guid.NewGuid(),
				Name = Guid.NewGuid().ToString(),
				CreatedByUserId = DatabaseInitialization.TestUser.Id,
			},
			dbTransaction);

		await dbTransaction.CommitAsync();
	}

	[Test]
	public async Task GetAllAsync()
	{
		var owners = await _ownerRepository.GetAllAsync(DatabaseInitialization.TestUser.Id);
		owners.Should().OnlyHaveUniqueItems();
	}
}

﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using Microsoft.Extensions.Logging.Abstractions;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class UnitRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private UnitRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(NullLogger<UnitRepository>.Instance, _dbConnection);
	}

	[Test]
	public async Task AddGetDelete_WithoutTransaction()
	{
		var unitToAdd = new UnitFaker(TestUser).Generate();

		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();
		var id = await _repository.AddAsync(unitToAdd, dbTransaction);

		var getUnit = await _repository.GetByIdAsync(id, TestUser.Id, dbTransaction);
		var findUnit = await _repository.FindByIdAsync(getUnit.Id, TestUser.Id, dbTransaction);
		var allUnits = await _repository.GetAsync(TestUser.Id, dbTransaction);

		var expectedUnit = unitToAdd with
		{
			Id = id,
			CreatedAt = getUnit.CreatedAt,
			ModifiedAt = getUnit.ModifiedAt,
		};

		using (new AssertionScope())
		{
			getUnit.Should().BeEquivalentTo(expectedUnit);
			findUnit.Should().BeEquivalentTo(expectedUnit);
			allUnits.Should().ContainSingle(unit => unit.Id == id).Which.Should().BeEquivalentTo(expectedUnit);
		}

		(await _repository.DeleteAsync(id, TestUser.Id, dbTransaction)).Should().Be(1);
		await dbTransaction.CommitAsync();

		var afterDelete = await _repository.FindByIdAsync(id, TestUser.Id);
		afterDelete.Should().BeNull();
	}

	[Test]
	public async Task AddGetDelete_WithTransaction()
	{
		var unitFaker = new UnitFaker(TestUser);
		await using var dbTransaction = await _dbConnection.BeginTransactionAsync();

		var parentUnit = unitFaker.Generate();
		var parentUnitId = await _repository.AddAsync(parentUnit, dbTransaction);

		var childUnit = unitFaker.GenerateUnique(parentUnit) with
		{
			ParentUnitId = parentUnitId,
			Multiplier = 1,
			InverseMultiplier = false,
		};

		var childUnitId = await _repository.AddAsync(childUnit, dbTransaction);

		var getUnit = await _repository.GetByIdAsync(childUnitId, TestUser.Id, dbTransaction);
		var expectedUnit = childUnit with
		{
			Id = childUnitId,
			CreatedAt = getUnit.CreatedAt,
			ModifiedAt = getUnit.ModifiedAt,
			ParentUnitId = parentUnitId,
		};

		getUnit.Should().BeEquivalentTo(expectedUnit);

		(await _repository.DeleteAsync(childUnitId, TestUser.Id, dbTransaction)).Should().Be(1);
		(await _repository.DeleteAsync(parentUnitId, TestUser.Id, dbTransaction)).Should().Be(1);

		await dbTransaction.CommitAsync();
	}
}

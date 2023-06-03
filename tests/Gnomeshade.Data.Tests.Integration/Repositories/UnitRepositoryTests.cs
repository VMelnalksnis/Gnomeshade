// Copyright 2021 Valters Melnalksnis
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

		var id = await _repository.AddAsync(unitToAdd);
		var getUnit = await _repository.GetByIdAsync(id, TestUser.Id);
		var findUnit = await _repository.FindByIdAsync(getUnit.Id, TestUser.Id);
		var allUnits = await _repository.GetAsync(TestUser.Id);

		var expectedUnit = unitToAdd with
		{
			Id = id,
			CreatedAt = getUnit.CreatedAt,
			ModifiedAt = getUnit.ModifiedAt,
		};

		getUnit.Should().BeEquivalentTo(expectedUnit);
		findUnit.Should().BeEquivalentTo(expectedUnit);
		allUnits.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedUnit);

		await _repository.DeleteAsync(id, TestUser.Id);

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

		var childUnit = unitFaker.GenerateUnique(parentUnit) with { ParentUnitId = parentUnitId, Multiplier = 1 };
		var childUnitId = await _repository.AddAsync(childUnit, dbTransaction);

		await dbTransaction.CommitAsync();

		var getUnit = await _repository.GetByIdAsync(childUnitId, TestUser.Id);
		var expectedUnit = childUnit with
		{
			Id = childUnitId,
			CreatedAt = getUnit.CreatedAt,
			ModifiedAt = getUnit.ModifiedAt,
			ParentUnitId = parentUnitId,
		};

		getUnit.Should().BeEquivalentTo(expectedUnit);

		await _repository.DeleteAsync(childUnitId, TestUser.Id);
		await _repository.DeleteAsync(parentUnitId, TestUser.Id);
	}
}

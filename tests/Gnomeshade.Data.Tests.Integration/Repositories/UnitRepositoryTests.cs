// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.TestingHelpers;

using NUnit.Framework;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories
{
	public class UnitRepositoryTests
	{
		private IDbConnection _dbConnection = null!;
		private UnitRepository _repository = null!;

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
		public async Task AddGetDelete_WithoutTransaction()
		{
			var unitToAdd = new UnitFaker(TestUser).Generate();

			var id = await _repository.AddAsync(unitToAdd);
			var getUnit = await _repository.GetByIdAsync(id);
			var findUnit = await _repository.FindByIdAsync(getUnit.Id);
			var allUnits = await _repository.GetAllAsync();

			var expectedUnit = unitToAdd with
			{
				Id = id,
				CreatedAt = getUnit.CreatedAt,
				ModifiedAt = getUnit.ModifiedAt,
			};

			getUnit.Should().BeEquivalentTo(expectedUnit);
			findUnit.Should().BeEquivalentTo(expectedUnit);
			allUnits.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedUnit);

			await _repository.DeleteAsync(id);

			var afterDelete = await _repository.FindByIdAsync(id);
			afterDelete.Should().BeNull();
		}

		[Test]
		public async Task AddGetDelete_WithTransaction()
		{
			var unitFaker = new UnitFaker(TestUser);
			using var dbTransaction = _dbConnection.BeginTransaction();

			var parentUnit = unitFaker.Generate();
			var parentUnitId = await _repository.AddAsync(parentUnit, dbTransaction);

			var childUnit = unitFaker.Generate() with { ParentUnitId = parentUnitId, Multiplier = 1 };
			var childUnitId = await _repository.AddAsync(childUnit, dbTransaction);

			dbTransaction.Commit();

			var getUnit = await _repository.GetByIdAsync(childUnitId);
			var expectedUnit = childUnit with
			{
				Id = childUnitId,
				CreatedAt = getUnit.CreatedAt,
				ModifiedAt = getUnit.ModifiedAt,
				ParentUnitId = parentUnitId,
			};

			getUnit.Should().BeEquivalentTo(expectedUnit);

			await _repository.DeleteAsync(childUnitId);
			await _repository.DeleteAsync(parentUnitId);
		}
	}
}

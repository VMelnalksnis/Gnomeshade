﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(UnitsController))]
public sealed class UnitsControllerTests : WebserverTests
{
	private IGnomeshadeClient _client = null!;
	private Unit _parentUnit = null!;

	public UnitsControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();

		var parentUnitId = Guid.NewGuid();
		_parentUnit = await PutAndGet(parentUnitId, CreateUniqueUnit());
	}

	[Test]
	public async Task Put_ShouldReturnConflictOnDuplicateName()
	{
		var creationModel = CreateUniqueUnit() with { ParentUnitId = _parentUnit.Id, Multiplier = 10 };
		await _client.PutUnitAsync(Guid.NewGuid(), creationModel);

		await ShouldThrowConflict(() => _client.PutUnitAsync(Guid.NewGuid(), creationModel));
	}

	[Test]
	public async Task Put()
	{
		var creationModel = CreateUniqueUnit() with { ParentUnitId = _parentUnit.Id, Multiplier = 10, Symbol = "kg" };
		var productId = Guid.NewGuid();
		var product = await PutAndGet(productId, creationModel);

		product.Name.Should().Be(creationModel.Name);
		product.Multiplier.Should().Be(creationModel.Multiplier);
		product.Symbol.Should().Be(creationModel.Symbol);

		var productWithoutChanges = await PutAndGet(productId, creationModel);

		productWithoutChanges.Should().BeEquivalentTo(product, WithoutModifiedAt);
		productWithoutChanges.ModifiedAt.Should().BeGreaterThanOrEqualTo(product.ModifiedAt);

		var changedCreationModel = creationModel with { ParentUnitId = null, Multiplier = null, Symbol = null };
		var productWithChanges = await PutAndGet(productId, changedCreationModel);

		productWithChanges.Should().BeEquivalentTo(product, WithoutModifiedAtAndParent);
		productWithChanges.Multiplier.Should().BeNull();

		var anotherCreationModel = CreateUniqueUnit();
		_ = await PutAndGet(productId, anotherCreationModel);
	}

	private static UnitCreation CreateUniqueUnit()
	{
		return new() { Name = Guid.NewGuid().ToString("N") };
	}

	private static EquivalencyAssertionOptions<Unit> WithoutModifiedAt(
		EquivalencyAssertionOptions<Unit> options)
	{
		return options.ComparingByMembers<Unit>().Excluding(model => model.ModifiedAt);
	}

	private static EquivalencyAssertionOptions<Unit> WithoutModifiedAtAndParent(
		EquivalencyAssertionOptions<Unit> options)
	{
		return WithoutModifiedAt(options)
			.Excluding(model => model.Multiplier)
			.Excluding(model => model.ParentUnitId)
			.Excluding(model => model.Symbol);
	}

	private async Task<Unit> PutAndGet(Guid id, UnitCreation creation)
	{
		await _client.PutUnitAsync(id, creation);
		return await _client.GetUnitAsync(id);
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Equivalency;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Products;

public class ProductControllerTests
{
	private IGnomeshadeClient _client = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Put()
	{
		var creationModel = CreateUniqueProduct() with { Description = "Foo" };
		var product = await PutAndGet(creationModel);

		product.Name.Should().Be(creationModel.Name);
		product.Description.Should().Be(creationModel.Description);

		var exception =
			await FluentActions
				.Awaiting(() => _client.PutProductAsync(creationModel))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>("name must be unique, and if was not specified for update");
		exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var creationModelWithId = creationModel with { Id = product.Id };
		var productWithoutChanges = await PutAndGet(creationModelWithId);

		productWithoutChanges.Should().BeEquivalentTo(product, WithoutModifiedAt);
		productWithoutChanges.ModifiedAt.Should().BeAfter(product.ModifiedAt, "currently models are not checked for differences");

		var changedCreationModel = creationModelWithId with { Description = null };
		var productWithChanges = await PutAndGet(changedCreationModel);

		productWithChanges.Should().BeEquivalentTo(product, WithoutModifiedAtAndDescription);
		productWithChanges.Description.Should().BeNull();

		var anotherCreationModel = CreateUniqueProduct();
		_ = await PutAndGet(anotherCreationModel);

		var creationModelWithDuplicateName = anotherCreationModel with
		{
			Name = product.Name,
		};

		exception =
			await FluentActions
				.Awaiting(() => _client.PutProductAsync(creationModelWithDuplicateName))
				.Should()
				.ThrowAsync<HttpRequestException>("name must be unique");
		exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	private static ProductCreationModel CreateUniqueProduct()
	{
		return new() { Name = Guid.NewGuid().ToString("N") };
	}

	private static EquivalencyAssertionOptions<Product> WithoutModifiedAt(
		EquivalencyAssertionOptions<Product> options)
	{
		return options.ComparingByMembers<Product>().Excluding(model => model.ModifiedAt);
	}

	private static EquivalencyAssertionOptions<Product> WithoutModifiedAtAndDescription(
		EquivalencyAssertionOptions<Product> options)
	{
		return WithoutModifiedAt(options).Excluding(model => model.Description);
	}

	private async Task<Product> PutAndGet(ProductCreationModel creationModel)
	{
		var productId = await _client.PutProductAsync(creationModel);
		return (await _client.GetProductsAsync()).Single(model => model.Id == productId);
	}
}

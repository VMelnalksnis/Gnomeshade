// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(ProjectsController))]
public sealed class ProjectsControllerTests(WebserverFixture fixture) : WebserverTests(fixture)
{
	private IGnomeshadeClient _client = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Put_ShouldReturnConflictOnDuplicateName()
	{
		var creationModel = CreateUniqueProject() with { };
		await _client.PutProjectAsync(Guid.NewGuid(), creationModel);

		await ShouldThrowConflict(() => _client.PutProjectAsync(Guid.NewGuid(), creationModel));
	}

	[Test]
	public async Task Put()
	{
		var parentProject = await CreateAndGet(CreateUniqueProject());

		var creationModel = CreateUniqueProject() with { ParentProjectId = parentProject.Id };
		var project = await CreateAndGet(creationModel);

		using (new AssertionScope())
		{
			project.Name.Should().Be(creationModel.Name);
			project.ParentProjectId.Should().Be(parentProject.Id);
		}

		var projectWithoutChanges = await PutAndGet(project.Id, creationModel);

		using (new AssertionScope())
		{
			projectWithoutChanges.Should().BeEquivalentTo(project, WithoutModifiedAt);
			projectWithoutChanges.ModifiedAt.Should().BeGreaterThanOrEqualTo(project.ModifiedAt);
		}

		var changedCreationModel = creationModel with { Name = Guid.NewGuid().ToString(), ParentProjectId = null };
		var projectWithChanges = await PutAndGet(project.Id, changedCreationModel);

		using (new AssertionScope())
		{
			projectWithChanges.Name.Should().NotBe(project.Name);
			projectWithChanges.ParentProjectId.Should().BeNull();
		}
	}

	[Test]
	public async Task Purchases_ShouldReturnNotFound()
	{
		await ShouldThrowNotFound(() => _client.GetProjectPurchasesAsync(Guid.NewGuid()));

		var project = await CreateAndGet(CreateUniqueProject());
		var unit = await _client.CreateUnitAsync();
		var category = await _client.CreateCategoryAsync();
		var product = await _client.CreateProductAsync(unit.Id, category.Id);
		var transaction = await _client.CreateTransactionAsync();
		var purchase = await _client.CreatePurchaseAsync(transaction.Id, product.Id);

		await _client.AddPurchaseToProjectAsync(project.Id, purchase.Id);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transaction.Id);
		var projectPurchases = await _client.GetProjectPurchasesAsync(project.Id);
		purchase = await _client.GetPurchaseAsync(purchase.Id);

		using (new AssertionScope())
		{
			projectPurchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
			projectPurchases.Should().BeEquivalentTo(detailedTransaction.Purchases);
		}

		await _client.RemovePurchaseFromProjectAsync(project.Id, purchase.Id);

		(await _client.GetProjectPurchasesAsync(project.Id)).Should().BeEmpty();
	}

	[Test]
	public async Task Delete_PurchaseReference()
	{
		var parentProject = await CreateAndGet(CreateUniqueProject());
		var project = await CreateAndGet(CreateUniqueProject() with { ParentProjectId = parentProject.Id });

		await ShouldThrowConflict(() => _client.DeleteProjectAsync(parentProject.Id));

		(await _client.GetProjectAsync(project.Id)).Should().BeEquivalentTo(project);

		await _client.DeleteProjectAsync(project.Id);
		await _client.DeleteProjectAsync(parentProject.Id);

		await ShouldThrowNotFound(() => _client.GetProjectAsync(project.Id));
	}

	private static ProjectCreation CreateUniqueProject() => new() { Name = Guid.NewGuid().ToString("N") };

	private static EquivalencyAssertionOptions<Project> WithoutModifiedAt(
		EquivalencyAssertionOptions<Project> options)
	{
		return options.ComparingByMembers<Project>().Excluding(model => model.ModifiedAt);
	}

	private async Task<Project> CreateAndGet(ProjectCreation creation)
	{
		var id = await _client.CreateProjectAsync(creation);
		return await _client.GetProjectAsync(id);
	}

	private async Task<Project> PutAndGet(Guid id, ProjectCreation creation)
	{
		await _client.PutProjectAsync(id, creation);
		return await _client.GetProjectAsync(id);
	}
}

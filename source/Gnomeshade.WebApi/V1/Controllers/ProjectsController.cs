// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.OpenApi;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on project entity.</summary>
public sealed class ProjectsController : CreatableBase<ProjectRepository, ProjectEntity, Project, ProjectCreation>
{
	private readonly PurchaseRepository _purchaseRepository;

	/// <summary>Initializes a new instance of the <see cref="ProjectsController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="ProjectEntity"/>.</param>
	/// <param name="purchaseRepository">The repository for performing CRUD operations on <see cref="PurchaseEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public ProjectsController(
		Mapper mapper,
		ProjectRepository repository,
		PurchaseRepository purchaseRepository,
		DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
		_purchaseRepository = purchaseRepository;
	}

	/// <inheritdoc cref="IProjectClient.GetProjectAsync"/>
	/// <response code="200">Successfully got the project.</response>
	/// <response code="404">Project with the specified id does not exist.</response>
	[ProducesResponseType<Project>(Status200OK)]
	public override Task<ActionResult<Project>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IProjectClient.GetProjectsAsync"/>
	/// <response code="200">Successfully got all projects.</response>
	[ProducesResponseType<List<Project>>(Status200OK)]
	public override Task<List<Project>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IProjectClient.CreateProjectAsync"/>
	/// <response code="201">A new project was created.</response>
	/// <response code="409">A project with the specified name already exists.</response>
	public override Task<ActionResult> Post(ProjectCreation project) =>
		base.Post(project);

	/// <inheritdoc cref="IProjectClient.PutProjectAsync"/>
	/// <response code="201">A new project was created.</response>
	/// <response code="204">An existing project was replaced.</response>
	/// <response code="409">A project with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] ProjectCreation product) =>
		base.Put(id, product);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="IProjectClient.DeleteProjectAsync"/>
	/// <response code="204">Project was successfully deleted.</response>
	/// <response code="404">Project with the specified id does not exist.</response>
	/// <response code="409">Project cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id)
		=> base.Delete(id);

	/// <inheritdoc cref="IProjectClient.GetProjectPurchasesAsync"/>
	/// <response code="200">Successfully got the purchases.</response>
	/// <response code="404">Project with the specified id does not exist.</response>
	[HttpGet("{id:guid}/Purchases")]
	[ProducesResponseType<List<Purchase>>(Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<List<Purchase>>> Purchases(Guid id, CancellationToken cancellationToken)
	{
		var userId = ApplicationUser.Id;

		var project = await Repository.FindByIdAsync(id, userId, AccessLevel.Read, cancellationToken);
		if (project is null)
		{
			return NotFound();
		}

		var purchases = await _purchaseRepository.GetAllForProject(id, userId, cancellationToken);
		var models = purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList();
		return Ok(models);
	}

	/// <inheritdoc cref="IProjectClient.AddPurchaseToProjectAsync"/>
	/// <response code="200">Successfully added the purchase to the project.</response>
	/// <response code="404">Project with the specified id does not exist.</response>
	[HttpPut("{id:guid}/Purchases/{purchaseId:guid}")]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> AddPurchase(Guid id, Guid purchaseId)
	{
		var userId = ApplicationUser.Id;
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var project = await Repository.FindByIdAsync(id, userId, dbTransaction, AccessLevel.Write);
		if (project is null)
		{
			return NotFound();
		}

		await Repository.AddPurchaseAsync(id, purchaseId, userId, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc cref="IProjectClient.RemovePurchaseFromProjectAsync"/>
	/// <response code="200">Successfully removed the purchase from the project.</response>
	/// <response code="404">Project with the specified id does not exist.</response>
	[HttpDelete("{id:guid}/Purchases/{purchaseId:guid}")]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> RemovePurchase(Guid id, Guid purchaseId)
	{
		var userId = ApplicationUser.Id;
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var project = await Repository.FindByIdAsync(id, userId, dbTransaction, AccessLevel.Write);
		if (project is null)
		{
			return NotFound();
		}

		await Repository.RemovePurchaseAsync(id, purchaseId, userId, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		ProjectCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var project = new ProjectEntity
		{
			Id = id,
			OwnerId = creation.OwnerId!.Value,
			ModifiedByUserId = user.Id,
			Name = creation.Name,
			ParentProjectId = creation.ParentProjectId,
		};

		return await Repository.UpdateAsync(project, dbTransaction) switch
		{
			1 => NoContent(),
			_ => StatusCode(Status403Forbidden),
		};
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(
		Guid id,
		ProjectCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictingProject = await Repository.FindByNameAsync(creation.Name, user.Id);
		if (conflictingProject is not null)
		{
			return Problem(
				"Project with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingProject.Id }),
				Status409Conflict);
		}

		var project = new ProjectEntity
		{
			Id = id,
			OwnerId = creation.OwnerId!.Value,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = creation.Name,
			ParentProjectId = creation.ParentProjectId,
		};

		_ = await Repository.AddAsync(project, dbTransaction);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

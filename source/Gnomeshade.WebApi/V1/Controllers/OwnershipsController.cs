// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Resource access management.</summary>
public sealed class OwnershipsController : FinanceControllerBase<OwnershipEntity, Ownership>
{
	private readonly OwnershipRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="OwnershipsController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="OwnershipEntity"/>.</param>
	public OwnershipsController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		OwnershipRepository repository)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetOwnershipsAsync"/>
	/// <response code="200">Successfully got all ownerships.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Ownership>), Status200OK)]
	public async Task<List<Ownership>> Get(CancellationToken cancellationToken)
	{
		var ownerships = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		return ownerships.Select(MapToModel).ToList();
	}

	/// <inheritdoc cref="IOwnerClient.PutOwnershipAsync"/>
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Put(Guid id, OwnershipCreation ownership)
	{
		var existingOwnership = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		if (existingOwnership is not null)
		{
			return await UpdateExistingOwnershipAsync(ownership, existingOwnership);
		}

		var conflictingOwnership = await _repository.FindByIdAsync(id);
		if (conflictingOwnership is null)
		{
			return await CreateNewOwnershipAsync(ownership, id);
		}

		return StatusCode(Status403Forbidden);
	}

	/// <inheritdoc cref="IOwnerClient.DeleteOwnershipAsync"/>
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		await _repository.DeleteAsync(id);
		return NoContent();
	}

	private async Task<ActionResult> CreateNewOwnershipAsync(OwnershipCreation model, Guid id)
	{
		var ownership = Mapper.Map<OwnershipEntity>(model) with { Id = id };
		await _repository.AddAsync(ownership);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult> UpdateExistingOwnershipAsync(
		OwnershipCreation model,
		OwnershipEntity existingOwnership)
	{
		var ownership = Mapper.Map<OwnershipEntity>(model) with { Id = existingOwnership.Id };
		_ = await _repository.UpdateAsync(ownership);
		return NoContent();
	}
}

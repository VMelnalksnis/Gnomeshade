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
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Owners;

/// <summary>CRUD operations on <see cref="Owner"/>.</summary>
public sealed class OwnersController : FinanceControllerBase<OwnerEntity, Owner>
{
	private readonly OwnerRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="OwnersController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="OwnerEntity"/>.</param>
	public OwnersController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<OwnersController> logger,
		OwnerRepository repository)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetOwnersAsync"/>
	[HttpGet]
	[ProducesResponseType(typeof(List<Owner>), Status200OK)]
	public async Task<List<Owner>> Get(CancellationToken cancellationToken)
	{
		var owners = await _repository.GetAllAsync(cancellationToken);
		return owners.Select(MapToModel).ToList();
	}

	/// <inheritdoc cref="IOwnerClient.PutOwnerAsync"/>
	[HttpPut("{id:guid}")]
	public async Task<ActionResult> Put(Guid id)
	{
		var existingOwner = (await _repository.GetAllAsync()).SingleOrDefault(owner => owner.Id == id);
		if (existingOwner is not null)
		{
			return NoContent();
		}

		await _repository.AddAsync(id);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	/// <inheritdoc cref="IOwnerClient.DeleteOwnerAsync"/>
	[HttpDelete("{id:guid}")]
	public async Task<ActionResult> Delete(Guid id)
	{
		var deletedCount = await _repository.DeleteAsync(id);
		return DeletedEntity<OwnerEntity>(id, deletedCount);
	}
}

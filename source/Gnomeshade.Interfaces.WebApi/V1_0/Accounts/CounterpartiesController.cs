// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

/// <summary>CRUD operations on account entity.</summary>
public sealed class CounterpartiesController : FinanceControllerBase<CounterpartyEntity, Counterparty>
{
	private readonly CounterpartyRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="CounterpartiesController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="CounterpartyEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	public CounterpartiesController(
		CounterpartyRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	/// <summary>Gets a single counterparty with the specified id.</summary>
	/// <param name="id">The id of the counterparty to get.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The counterparty if it was found, otherwise <see cref="NotFoundResult"/>.</returns>
	/// <response code="200">Counterparty with the specified id exists.</response>
	/// <response code="404">Counterparty with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesStatus404NotFound]
	public Task<ActionResult<Counterparty>> Get(Guid id, CancellationToken cancellationToken)
	{
		return Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	/// <summary>Gets the counterparty linked to the currently authenticated user.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The counterparty linked to the user.</returns>
	/// <response code="200">Successfully got the counterparty.</response>
	/// <response code="404">No counterparty was linked to the user.</response>
	[HttpGet("me")]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Counterparty>> GetMe(CancellationToken cancellationToken)
	{
		return await Find(() =>
			_repository.FindByIdAsync(ApplicationUser.CounterpartyId, ApplicationUser.Id, cancellationToken));
	}

	/// <summary>Gets all counterparties.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all counterparties.</returns>
	/// <response code="200">Successfully got all counterparties.</response>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<Counterparty>>> GetAll(CancellationToken cancellationToken)
	{
		var counterparties = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var models = counterparties.Select(party => Mapper.Map<Counterparty>(party)).ToList();
		return Ok(models);
	}

	/// <summary>Creates a new counterparty.</summary>
	/// <param name="creationModel">The counterparty that will be created.</param>
	/// <returns><see cref="CreatedAtActionResult"/> with the id of the counterparty.</returns>
	/// <response code="201">Counterparty was successfully created.</response>
	[HttpPost]
	[ProducesResponseType(Status201Created)]
	public async Task<ActionResult<Guid>> Post([FromBody, BindRequired] CounterpartyCreationModel creationModel)
	{
		var counterparty = Mapper.Map<CounterpartyEntity>(creationModel) with
		{
			Id = Guid.NewGuid(),
			OwnerId = ApplicationUser.Id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			NormalizedName = creationModel.Name!.ToUpperInvariant(),
		};

		var id = await _repository.AddAsync(counterparty);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	/// <summary>Creates a new counterparty, or replaces and existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the counterparty.</param>
	/// <param name="model">The counterparty to create or update.</param>
	/// <returns>A status code indicating the result of the action.</returns>
	/// <response code="201">A new counterparty was created.</response>
	/// <response code="204">An existing counterparty was replaced.</response>
	/// <response code="409">A counterparty with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	public async Task<ActionResult> Put(Guid id, [FromBody] CounterpartyCreationModel model)
	{
		var existingCounterparty = await _repository.FindByIdAsync(id, ApplicationUser.Id);

		return existingCounterparty is null
			? await CreateCounterpartyAsync(id, model, ApplicationUser)
			: await UpdateCounterpartyAsync(id, model, ApplicationUser);
	}

	/// <summary>Merges one counterparty into another.</summary>
	/// <param name="targetId">The id of the counterparty in to which to merge.</param>
	/// <param name="sourceId">The id of the counterparty which to merge into the other one.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <response code="204">Counterparties were successfully merged.</response>
	[HttpPost("{targetId:guid}/Merge/{sourceId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> Merge(Guid targetId, Guid sourceId)
	{
		await _repository.MergeAsync(targetId, sourceId, ApplicationUser.Id);
		return NoContent();
	}

	private async Task<ActionResult> CreateCounterpartyAsync(Guid id, CounterpartyCreationModel model, UserEntity user)
	{
		var normalizedName = model.Name!.ToUpperInvariant();
		var conflictingCounterparty = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingCounterparty is not null)
		{
			return Problem(
				"Counterparty with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingCounterparty.Id }),
				Status409Conflict);
		}

		var counterparty = Mapper.Map<CounterpartyEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = normalizedName,
		};

		_ = await _repository.AddAsync(counterparty);
		return CreatedAtAction(nameof(Get), new { id }, null);
	}

	private async Task<ActionResult> UpdateCounterpartyAsync(Guid id, CounterpartyCreationModel model, UserEntity user)
	{
		var counterparty = Mapper.Map<CounterpartyEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = model.Name!.ToUpperInvariant(),
		};

		var rows = await _repository.UpdateAsync(counterparty);
		Debug.Assert(rows > 0, "No rows were changed after update");
		return NoContent();
	}
}

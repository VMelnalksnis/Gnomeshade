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
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
	/// <param name="logger">Logger for logging in the specified category.</param>
	public CounterpartiesController(
		CounterpartyRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<CounterpartiesController> logger)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IAccountClient.GetCounterpartyAsync"/>
	/// <response code="200">Counterparty with the specified id exists.</response>
	/// <response code="404">Counterparty with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Counterparty), Status200OK)]
	[ProducesStatus404NotFound]
	public Task<ActionResult<Counterparty>> Get(Guid id, CancellationToken cancellationToken)
	{
		return Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	/// <inheritdoc cref="IAccountClient.GetMyCounterpartyAsync"/>
	/// <response code="200">Successfully got the counterparty.</response>
	/// <response code="404">No counterparty was linked to the user.</response>
	[HttpGet("me")]
	[ProducesResponseType(typeof(Counterparty), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Counterparty>> GetMe(CancellationToken cancellationToken)
	{
		return await Find(() => _repository.FindByIdAsync(ApplicationUser.CounterpartyId, ApplicationUser.Id, cancellationToken));
	}

	/// <inheritdoc cref="IAccountClient.GetCounterpartiesAsync"/>
	/// <response code="200">Successfully got all counterparties.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Counterparty>), Status200OK)]
	public async Task<ActionResult<List<Counterparty>>> GetAll(CancellationToken cancellationToken)
	{
		var counterparties = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var models = counterparties.Select(party => Mapper.Map<Counterparty>(party)).ToList();
		return Ok(models);
	}

	/// <inheritdoc cref="IAccountClient.CreateCounterpartyAsync"/>
	/// <response code="201">Counterparty was successfully created.</response>
	[HttpPost]
	[ProducesResponseType(typeof(Guid), Status201Created)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult<Guid>> Post([FromBody] CounterpartyCreationModel counterparty)
	{
		var conflictResult = await GetConflictResult(counterparty, ApplicationUser);
		if (conflictResult is not null)
		{
			return conflictResult;
		}

		var entity = Mapper.Map<CounterpartyEntity>(counterparty) with
		{
			Id = Guid.NewGuid(),
			OwnerId = ApplicationUser.Id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			NormalizedName = counterparty.Name!.ToUpperInvariant(),
		};

		var id = await _repository.AddAsync(entity);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	/// <inheritdoc cref="IAccountClient.PutCounterpartyAsync"/>
	/// <response code="201">A new counterparty was created.</response>
	/// <response code="204">An existing counterparty was replaced.</response>
	/// <response code="409">A counterparty with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(typeof(Guid), Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody] CounterpartyCreationModel counterparty)
	{
		var existingCounterparty = await _repository.FindByIdAsync(id, ApplicationUser.Id);

		return existingCounterparty is null
			? await CreateCounterpartyAsync(id, counterparty, ApplicationUser)
			: await UpdateCounterpartyAsync(id, counterparty, ApplicationUser);
	}

	/// <inheritdoc cref="IAccountClient.MergeCounterpartiesAsync"/>
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
		var conflictResult = await GetConflictResult(model, user);
		if (conflictResult is not null)
		{
			return conflictResult;
		}

		var counterparty = Mapper.Map<CounterpartyEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = model.Name!.ToUpperInvariant(),
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

	private async Task<ActionResult?> GetConflictResult(CounterpartyCreationModel model, UserEntity user)
	{
		var normalizedName = model.Name!.ToUpperInvariant();
		var conflictingCounterparty = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingCounterparty is null)
		{
			return null;
		}

		return Problem(
			"Counterparty with the specified name already exists",
			Url.Action(nameof(Get), new { conflictingCounterparty.Id }),
			Status409Conflict);
	}
}

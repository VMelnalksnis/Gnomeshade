﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.OpenApi;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on account entity.</summary>
public sealed class CounterpartiesController : CreatableBase<CounterpartyRepository, CounterpartyEntity, Counterparty, CounterpartyCreation>
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartiesController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="CounterpartyEntity"/>.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public CounterpartiesController(CounterpartyRepository repository, Mapper mapper, DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
	}

	/// <inheritdoc cref="IAccountClient.GetCounterpartyAsync"/>
	/// <response code="200">Counterparty with the specified id exists.</response>
	/// <response code="404">Counterparty with the specified id does not exist.</response>
	[ProducesResponseType<Counterparty>(Status200OK)]
	public override Task<ActionResult<Counterparty>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IAccountClient.GetMyCounterpartyAsync"/>
	/// <response code="200">Successfully got the counterparty.</response>
	/// <response code="404">No counterparty was linked to the user.</response>
	[HttpGet("me")]
	[ProducesResponseType<Counterparty>(Status200OK)]
	[ProducesStatus404NotFound]
	public Task<ActionResult<Counterparty>> GetMe(CancellationToken cancellationToken)
	{
		return Find(() => Repository.FindByIdAsync(ApplicationUser.CounterpartyId, ApplicationUser.Id, AccessLevel.Read, cancellationToken));
	}

	/// <inheritdoc cref="IAccountClient.GetCounterpartiesAsync"/>
	/// <response code="200">Successfully got all counterparties.</response>
	[ProducesResponseType<List<Counterparty>>(Status200OK)]
	public override Task<List<Counterparty>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IAccountClient.CreateCounterpartyAsync"/>
	/// <response code="201">Counterparty was successfully created.</response>
	public override Task<ActionResult> Post([FromBody] CounterpartyCreation counterparty) =>
		base.Post(counterparty);

	/// <inheritdoc cref="IAccountClient.PutCounterpartyAsync"/>
	/// <response code="201">A new counterparty was created.</response>
	/// <response code="204">An existing counterparty was replaced.</response>
	/// <response code="403">A counterparty with the specified id already exists, but you do not have access to it.</response>
	/// <response code="409">A counterparty with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] CounterpartyCreation counterparty) =>
		base.Put(id, counterparty);

	/// <inheritdoc cref="IAccountClient.MergeCounterpartiesAsync"/>
	/// <response code="204">Counterparties were successfully merged.</response>
	[HttpPost("{targetId:guid}/Merge/{sourceId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> Merge(Guid targetId, Guid sourceId)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();
		await Repository.MergeAsync(targetId, sourceId, ApplicationUser.Id, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		CounterpartyCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictResult = await GetConflictResult(creation, user, dbTransaction, id);
		if (conflictResult is not null)
		{
			return conflictResult;
		}

		var counterparty = Mapper.Map<CounterpartyEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		return await Repository.UpdateAsync(counterparty, dbTransaction) switch
		{
			1 => NoContent(),
			_ => StatusCode(Status403Forbidden),
		};
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(
		Guid id,
		CounterpartyCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictResult = await GetConflictResult(creation, user, dbTransaction);
		if (conflictResult is not null)
		{
			return conflictResult;
		}

		var counterparty = Mapper.Map<CounterpartyEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
		};

		_ = await Repository.AddAsync(counterparty, dbTransaction);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult?> GetConflictResult(
		CounterpartyCreation model,
		UserEntity user,
		DbTransaction dbTransaction,
		Guid? existingId = null)
	{
		var conflictingCounterparty = await Repository.FindByNameAsync(model.Name!, user.Id, dbTransaction);
		if (conflictingCounterparty is null || conflictingCounterparty.Id == existingId)
		{
			return null;
		}

		return Problem(
			"Counterparty with the specified name already exists",
			Url.Action(nameof(Get), new { conflictingCounterparty.Id }),
			Status409Conflict);
	}
}

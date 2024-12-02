// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Models.Transactions;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on transaction schedule entity.</summary>
[Route("api/v{version:apiVersion}/Transactions/Schedules")]
public sealed class TransactionSchedulesController
	: CreatableBase<TransactionScheduleRepository, TransactionScheduleEntity, TransactionSchedule,
		TransactionScheduleCreation>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionSchedulesController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="ProjectEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public TransactionSchedulesController(
		Mapper mapper,
		TransactionScheduleRepository repository,
		DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
	}

	/// <inheritdoc cref="ITransactionClient.GetTransactionSchedule"/>
	/// <response code="200">Successfully got the transaction schedule.</response>
	/// <response code="404">Project with the specified id does not exist.</response>
	[ProducesResponseType<Project>(Status200OK)]
	public override Task<ActionResult<TransactionSchedule>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetTransactionSchedules"/>
	/// <response code="200">Successfully got all transaction schedules.</response>
	[ProducesResponseType<List<TransactionSchedule>>(Status200OK)]
	public override Task<List<TransactionSchedule>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.CreateTransactionSchedule"/>
	/// <response code="201">A new transaction schedule was created.</response>
	/// <response code="409">A transaction schedule with the specified name already exists.</response>
	public override Task<ActionResult> Post(TransactionScheduleCreation schedule) =>
		base.Post(schedule);

	/// <inheritdoc cref="ITransactionClient.PutTransactionSchedule"/>
	/// <response code="201">A new transaction schedule was created.</response>
	/// <response code="204">An existing transaction schedule was replaced.</response>
	/// <response code="409">A transaction schedule with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] TransactionScheduleCreation schedule) =>
		base.Put(id, schedule);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="ITransactionClient.DeleteTransactionSchedule"/>
	/// <response code="204">Transaction schedule was successfully deleted.</response>
	/// <response code="404">Transaction schedule with the specified id does not exist.</response>
	/// <response code="409">Transaction schedule cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id)
		=> base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		TransactionScheduleCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var schedule = new TransactionScheduleEntity
		{
			Id = id,
			OwnerId = creation.OwnerId!.Value,
			ModifiedByUserId = user.Id,
			Name = creation.Name,
			StartingAt = creation.StartingAt,
			Period = creation.Period,
			Count = creation.Count,
		};

		return await Repository.UpdateAsync(schedule, dbTransaction) switch
		{
			1 => NoContent(),
			_ => StatusCode(Status403Forbidden),
		};

		// todo planned transactions
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(
		Guid id,
		TransactionScheduleCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictingSchedule = await Repository.FindByNameAsync(creation.Name, user.Id);
		if (conflictingSchedule is not null)
		{
			return Problem(
				"Schedule with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingSchedule.Id }),
				Status409Conflict);
		}

		var schedule = new TransactionScheduleEntity
		{
			Id = id,
			OwnerId = creation.OwnerId!.Value,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = creation.Name,
			StartingAt = creation.StartingAt,
			Period = creation.Period,
			Count = creation.Count,
		};

		_ = await Repository.AddAsync(schedule, dbTransaction);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

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
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products;

/// <summary>CRUD operations on unit entity.</summary>
public sealed class UnitsController : FinanceControllerBase<UnitEntity, Unit>
{
	private readonly UnitRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="UnitsController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="UnitEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public UnitsController(
		UnitRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<UnitsController> logger)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IProductClient.GetUnitAsync"/>
	/// <response code="200">Unit with the specified id exists.</response>
	/// <response code="404">Unit with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Unit), Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
	public async Task<ActionResult<Unit>> Get(Guid id, CancellationToken cancellationToken)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	/// <inheritdoc cref="IProductClient.GetUnitsAsync"/>
	/// <response code="200">Successfully got all units.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Unit>), Status200OK)]
	public async Task<ActionResult<List<Unit>>> GetAll(CancellationToken cancellationToken)
	{
		var units = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var models = units.Select(MapToModel).ToList();
		return Ok(models);
	}

	/// <inheritdoc cref="IProductClient.PutUnitAsync"/>
	/// <response code="201">A new unit was created.</response>
	/// <response code="204">An existing unit was replaced.</response>
	/// <response code="409">A unit with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody, BindRequired] UnitCreationModel model)
	{
		var existingUnit = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		return existingUnit is null
			? await PutNewUnitAsync(model, ApplicationUser, id)
			: await UpdateExistingUnitAsync(model, ApplicationUser, id);
	}

	private async Task<ActionResult> PutNewUnitAsync(UnitCreationModel model, UserEntity user, Guid id)
	{
		var normalizedName = model.Name!.ToUpperInvariant();
		var conflictingUnit = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingUnit is not null)
		{
			return Problem(
				"Unit with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingUnit.Id }),
				Status409Conflict);
		}

		var unit = Mapper.Map<UnitEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = normalizedName,
		};

		_ = await _repository.AddAsync(unit);
		return CreatedAtAction(nameof(Get), new { id }, null);
	}

	private async Task<ActionResult> UpdateExistingUnitAsync(UnitCreationModel model, UserEntity user, Guid id)
	{
		var unit = Mapper.Map<UnitEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
			NormalizedName = model.Name!.ToUpperInvariant(),
			ModifiedByUserId = user.Id,
		};

		var rowCount = await _repository.UpdateAsync(unit);
		Debug.Assert(rowCount > 0, "No rows were changed after update");
		return NoContent();
	}
}

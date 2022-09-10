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
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Products;

/// <summary>CRUD operations on unit entity.</summary>
public sealed class UnitsController : CreatableBase<UnitRepository, UnitEntity, Unit, UnitCreation>
{
	/// <summary>Initializes a new instance of the <see cref="UnitsController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="UnitEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public UnitsController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<UnitsController> logger,
		UnitRepository repository,
		DbConnection dbConnection)
		: base(applicationUserContext, mapper, logger, repository, dbConnection)
	{
	}

	/// <inheritdoc cref="IProductClient.GetUnitAsync"/>
	/// <response code="200">Unit with the specified id exists.</response>
	/// <response code="404">Unit with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Unit), Status200OK)]
	public override Task<ActionResult<Unit>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IProductClient.GetUnitsAsync"/>
	/// <response code="200">Successfully got all units.</response>
	[ProducesResponseType(typeof(List<Unit>), Status200OK)]
	public override Task<List<Unit>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IProductClient.PutUnitAsync"/>
	/// <response code="201">A new unit was created.</response>
	/// <response code="204">An existing unit was replaced.</response>
	/// <response code="409">A unit with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody, BindRequired] UnitCreation unit) =>
		base.Put(id, unit);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, UnitCreation creation, UserEntity user)
	{
		var unit = Mapper.Map<UnitEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		await Repository.UpdateAsync(unit);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, UnitCreation creation, UserEntity user)
	{
		var conflictingUnit = await Repository.FindByNameAsync(creation.Name!, user.Id);
		if (conflictingUnit is not null)
		{
			return Problem(
				"Unit with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingUnit.Id }),
				Status409Conflict);
		}

		var unit = Mapper.Map<UnitEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
		};

		_ = await Repository.AddAsync(unit);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

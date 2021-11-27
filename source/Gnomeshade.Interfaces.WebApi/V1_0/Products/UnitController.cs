// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products;

[SuppressMessage(
	"ReSharper",
	"AsyncConverter.ConfigureAwaitHighlighting",
	Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
public sealed class UnitController : FinanceControllerBase<UnitEntity, Unit>
{
	private readonly UnitRepository _repository;

	public UnitController(
		UnitRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
	public async Task<ActionResult<Unit>> Get(Guid id, CancellationToken cancellationToken)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	[HttpGet]
	[ProducesResponseType(Status200OK)]
	public async Task<ActionResult<List<Unit>>> GetAll(CancellationToken cancellationToken)
	{
		var units = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var models = units.Select(MapToModel).ToList();
		return Ok(models);
	}

	[HttpPost]
	[ProducesResponseType(Status201Created)]
	public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] UnitCreationModel creationModel)
	{
		var unit = Mapper.Map<UnitEntity>(creationModel) with
		{
			OwnerId = ApplicationUser.Id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			NormalizedName = creationModel.Name!.ToUpperInvariant(),
		};

		var id = await _repository.AddAsync(unit);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

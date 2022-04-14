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
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0;

/// <summary>CRUD operations on link entity.</summary>
public sealed class LinksController : FinanceControllerBase<LinkEntity, Link>
{
	private readonly LinkRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="LinksController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="LinkEntity"/>.</param>
	public LinksController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<LinksController> logger,
		LinkRepository repository)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IGnomeshadeClient.GetLinksAsync"/>
	/// <response code="200">Successfully got all links.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Link>), Status200OK)]
	public async Task<List<Link>> GetAll(CancellationToken cancellationToken)
	{
		var links = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var models = links.Select(MapToModel).ToList();
		return models;
	}

	/// <inheritdoc cref="IGnomeshadeClient.GetLinkAsync"/>
	/// <response code="200">Link with the specified id exists.</response>
	/// <response code="404">Link with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Link), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Link>> Get(Guid id, CancellationToken cancellationToken)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	/// <inheritdoc cref="IGnomeshadeClient.PutLinkAsync"/>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult<Link>> Put(Guid id, LinkCreation link, CancellationToken cancellationToken)
	{
		var existingLink = await _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken);
		var linkToCreate = existingLink ?? new()
		{
			Id = id,
			OwnerId = ApplicationUser.Id,
			CreatedAt = SystemClock.Instance.GetCurrentInstant(),
			CreatedByUserId = ApplicationUser.Id,
		};

		linkToCreate.ModifiedAt = SystemClock.Instance.GetCurrentInstant();
		linkToCreate.ModifiedByUserId = ApplicationUser.Id;
		linkToCreate.Uri = link.Uri!.ToString();

		if (existingLink is null)
		{
			await _repository.AddAsync(linkToCreate);
			return CreatedAtAction(nameof(Get), new { id }, null);
		}

		await _repository.UpdateAsync(linkToCreate);
		return NoContent();
	}

	/// <inheritdoc cref="IGnomeshadeClient.DeleteLinkAsync"/>
	/// <response code="204">Link was successfully deleted.</response>
	/// <response code="404">Link with the specified id does not exist.</response>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<StatusCodeResult> Delete(Guid id)
	{
		var rows = await _repository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<Link>(id, rows);
	}
}

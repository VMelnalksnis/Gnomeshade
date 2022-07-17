﻿// Copyright 2021 Valters Melnalksnis
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
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0;

/// <summary>CRUD operations on link entity.</summary>
public sealed class LinksController : CreatableBase<LinkRepository, LinkEntity, Link, LinkCreation>
{
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
		: base(applicationUserContext, mapper, logger, repository)
	{
	}

	/// <inheritdoc cref="IGnomeshadeClient.GetLinksAsync"/>
	/// <response code="200">Successfully got all links.</response>
	[ProducesResponseType(typeof(List<Link>), Status200OK)]
	public override Task<List<Link>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IGnomeshadeClient.GetLinkAsync"/>
	/// <response code="200">Link with the specified id exists.</response>
	/// <response code="404">Link with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Link), Status200OK)]
	public override Task<ActionResult<Link>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IGnomeshadeClient.PutLinkAsync"/>
	public override Task<ActionResult> Put(Guid id, LinkCreation link) =>
		base.Put(id, link);

	/// <inheritdoc cref="IGnomeshadeClient.DeleteLinkAsync"/>
	/// <response code="204">Link was successfully deleted.</response>
	/// <response code="404">Link with the specified id does not exist.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<StatusCodeResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, LinkCreation creation, UserEntity user)
	{
		var conflictResult = await GetConflictResult(creation, user);
		if (conflictResult is not null)
		{
			return conflictResult;
		}

		var linkToCreate = new LinkEntity
		{
			Id = id,
			OwnerId = creation.OwnerId!.Value,
			ModifiedByUserId = user.Id,
			Uri = creation.Uri!.ToString(),
		};

		var updatedCount = await Repository.UpdateAsync(linkToCreate);
		if (updatedCount is not 1)
		{
			throw new ApplicationException($"Unexpected update count {updatedCount}");
		}

		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, LinkCreation creation, UserEntity user)
	{
		var conflictResult = await GetConflictResult(creation, user);
		if (conflictResult is not null)
		{
			return conflictResult;
		}

		var link = new LinkEntity
		{
			Id = id,
			OwnerId = creation.OwnerId!.Value,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Uri = creation.Uri!.ToString(),
		};

		_ = await Repository.AddAsync(link);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult?> GetConflictResult(LinkCreation creation, UserEntity user, Guid? existingId = null)
	{
		var links = await Repository.GetAllAsync(user.Id);
		var conflictingLink = links.FirstOrDefault(link => link.Uri == creation.Uri?.ToString());
		if (conflictingLink is null || conflictingLink.Id == existingId)
		{
			return null;
		}

		return Problem(
			"Link with the specified URI already exists",
			Url.Action(nameof(Get), new { conflictingLink.Id }),
			Status409Conflict);
	}
}

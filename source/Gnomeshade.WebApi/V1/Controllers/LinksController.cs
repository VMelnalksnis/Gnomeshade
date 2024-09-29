// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on link entity.</summary>
public sealed class LinksController(Mapper mapper, LinkRepository repository, DbConnection dbConnection)
	: CreatableBase<LinkRepository, LinkEntity, Link, LinkCreation>(mapper, repository, dbConnection)
{
	/// <inheritdoc cref="IGnomeshadeClient.GetLinksAsync"/>
	/// <response code="200">Successfully got all links.</response>
	[ProducesResponseType<List<Link>>(Status200OK)]
	public override Task<List<Link>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IGnomeshadeClient.GetLinkAsync"/>
	/// <response code="200">Link with the specified id exists.</response>
	/// <response code="404">Link with the specified id does not exist.</response>
	[ProducesResponseType<Link>(Status200OK)]
	public override Task<ActionResult<Link>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IGnomeshadeClient.PutLinkAsync"/>
	public override Task<ActionResult> Put(Guid id, LinkCreation link) =>
		base.Put(id, link);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="IGnomeshadeClient.DeleteLinkAsync"/>
	/// <response code="204">Link was successfully deleted.</response>
	/// <response code="404">Link with the specified id does not exist.</response>
	/// <response code="409">Link cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		LinkCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictResult = await GetConflictResult(creation, user, dbTransaction, id);
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

		return await Repository.UpdateAsync(linkToCreate, dbTransaction) switch
		{
			1 => NoContent(),
			_ => StatusCode(Status403Forbidden),
		};
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(
		Guid id,
		LinkCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictResult = await GetConflictResult(creation, user, dbTransaction);
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

		_ = await Repository.AddAsync(link, dbTransaction);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult?> GetConflictResult(
		LinkCreation creation,
		UserEntity user,
		DbTransaction dbTransaction,
		Guid? existingId = null)
	{
		var links = await Repository.GetAsync(user.Id, dbTransaction);
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

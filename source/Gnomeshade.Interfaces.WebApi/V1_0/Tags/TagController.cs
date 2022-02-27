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
using Gnomeshade.Interfaces.WebApi.Models.Tags;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Tags;

/// <summary>CRUD operations on tags.</summary>
public sealed class TagController : FinanceControllerBase<TagEntity, Tag>
{
	private readonly TagRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="TagController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TagEntity"/>.</param>
	public TagController(ApplicationUserContext applicationUserContext, Mapper mapper, TagRepository repository)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="ITagClient.GetTagsAsync"/>
	[HttpGet]
	[ProducesResponseType(typeof(List<Tag>), Status200OK)]
	public async Task<ActionResult<List<Tag>>> Get(CancellationToken cancellationToken = default)
	{
		var tagEntities = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var tags = tagEntities.Select(MapToModel).ToList();
		return Ok(tags);
	}

	/// <inheritdoc cref="ITagClient.GetTagAsync"/>
	/// <response code="200">Successfully got the tag.</response>
	/// <response code="404">Tag with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Tag), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Tag>> Get(Guid id, CancellationToken cancellationToken = default)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	/// <inheritdoc cref="ITagClient.PutTagAsync"/>
	/// <response code="201">A new product was created.</response>
	/// <response code="204">An existing product was replaced.</response>
	/// <response code="409">A product with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody] TagCreation tag)
	{
		var existingTag = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		return existingTag is null
			? await CreateTagAsync(tag, ApplicationUser, id)
			: await UpdateTagAsync(tag, ApplicationUser, id);
	}

	/// <inheritdoc cref="ITagClient.DeleteTagAsync"/>
	/// <response code="204">The tag was deleted successfully.</response>
	[HttpDelete("{id:guid}")]
	public async Task<StatusCodeResult> Delete(Guid id)
	{
		await _repository.DeleteAsync(id, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITagClient.TagTagAsync"/>
	/// <response code="204">The tag was tagged successfully.</response>
	[HttpPut("{id:guid}/Tag/{tagId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<StatusCodeResult> Tag(Guid id, Guid tagId)
	{
		await _repository.TagAsync(id, tagId, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITagClient.UntagTagAsync"/>
	/// <response code="204">The tag was untagged successfully.</response>
	[HttpDelete("{id:guid}/Tag/{tagId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<StatusCodeResult> Untag(Guid id, Guid tagId)
	{
		await _repository.UntagAsync(id, tagId, ApplicationUser.Id);
		return NoContent();
	}

	private async Task<ActionResult> CreateTagAsync(TagCreation model, UserEntity user, Guid id)
	{
		var normalizedName = model.Name.ToUpperInvariant();
		var conflictingTag = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingTag is not null)
		{
			return Problem(
				"Tag with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingTag.Id }),
				Status409Conflict);
		}

		var tag = Mapper.Map<TagEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = normalizedName,
		};

		_ = await _repository.AddAsync(tag);
		return CreatedAtAction(nameof(Get), new { id }, null);
	}

	private async Task<NoContentResult> UpdateTagAsync(TagCreation model, UserEntity user, Guid id)
	{
		var tag = Mapper.Map<TagEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
			NormalizedName = model.Name.ToUpperInvariant(),
			ModifiedByUserId = user.Id,
		};

		var x = await _repository.UpdateAsync(tag);
		Debug.Assert(x > 0, "No rows were changed after update");
		return NoContent();
	}
}

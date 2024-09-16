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
using Gnomeshade.WebApi.Models.Owners;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Resource access management.</summary>
public sealed class OwnershipsController(Mapper mapper, OwnershipRepository repository, DbConnection dbConnection)
	: CreatableBase<OwnershipRepository, OwnershipEntity, Ownership, OwnershipCreation>(mapper, repository, dbConnection)
{
	/// <inheritdoc cref="IOwnerClient.GetOwnershipsAsync"/>
	/// <response code="200">Successfully got all ownerships.</response>
	[ProducesResponseType<List<Ownership>>(Status200OK)]
	public override Task<List<Ownership>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IOwnerClient.PutOwnershipAsync"/>
	/// <response code="201">A new ownership was created.</response>
	/// <response code="204">An existing ownership was replaced.</response>
	public override Task<ActionResult> Put(Guid id, OwnershipCreation ownership) =>
		base.Put(id, ownership);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="IOwnerClient.DeleteOwnershipAsync"/>
	/// <response code="204">Ownership was deleted successfully.</response>
	/// <response code="404">Ownership with the specified id does not exist.</response>
	/// <response code="409">Ownership cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		OwnershipCreation creation,
		UserEntity user)
	{
		var ownership = Mapper.Map<OwnershipEntity>(creation) with { Id = id };
		return await Repository.UpdateAsync(ownership) switch
		{
			1 => NoContent(),
			_ => StatusCode(Status403Forbidden),
		};
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, OwnershipCreation creation, UserEntity user)
	{
		var ownership = Mapper.Map<OwnershipEntity>(creation) with { Id = id };
		await Repository.AddAsync(ownership);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

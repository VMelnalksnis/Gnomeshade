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
public sealed class OwnershipsController : CreatableBase<OwnershipRepository, OwnershipEntity, Ownership, OwnershipCreation>
{
	private readonly OwnershipRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="OwnershipsController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="OwnershipEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public OwnershipsController(Mapper mapper, OwnershipRepository repository, DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetOwnershipsAsync"/>
	/// <response code="200">Successfully got all ownerships.</response>
	[ProducesResponseType(typeof(List<Ownership>), Status200OK)]
	public override Task<List<Ownership>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IOwnerClient.PutOwnershipAsync"/>
	/// <response code="201">A new ownership was created.</response>
	/// <response code="204">An existing ownership was replaced.</response>
	public override Task<ActionResult> Put(Guid id, OwnershipCreation ownership) =>
		base.Put(id, ownership);

	/// <inheritdoc cref="IOwnerClient.DeleteOwnershipAsync"/>
	/// <response code="204">The ownership was deleted successfully.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		OwnershipCreation creation,
		UserEntity user)
	{
		var ownership = Mapper.Map<OwnershipEntity>(creation) with { Id = id };
		await _repository.UpdateAsync(ownership);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, OwnershipCreation creation, UserEntity user)
	{
		var ownership = Mapper.Map<OwnershipEntity>(creation) with { Id = id };
		await _repository.AddAsync(ownership);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

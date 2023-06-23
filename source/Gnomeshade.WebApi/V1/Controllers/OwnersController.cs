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

/// <summary>CRUD operations on <see cref="Owner"/>.</summary>
public sealed class OwnersController : CreatableBase<OwnerRepository, OwnerEntity, Owner, OwnerCreation>
{
	private readonly OwnerRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="OwnersController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="OwnerEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public OwnersController(Mapper mapper, OwnerRepository repository, DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetOwnersAsync"/>
	/// <response code="200">Successfully got the owners.</response>
	[ProducesResponseType(typeof(List<Owner>), Status200OK)]
	public override Task<List<Owner>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IOwnerClient.PutOwnerAsync"/>
	/// <response code="201">A new owner was created.</response>
	/// <response code="204">An existing owner was replaced.</response>
	/// <response code="409">An owner with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, OwnerCreation owner) =>
		base.Put(id, owner);

	/// <inheritdoc cref="IOwnerClient.DeleteOwnerAsync"/>
	/// <response code="204">The owner was deleted successfully.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override Task<ActionResult> UpdateExistingAsync(Guid id, OwnerCreation creation, UserEntity user)
	{
		// todo
		return Task.FromResult<ActionResult>(NoContent());
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, OwnerCreation creation, UserEntity user)
	{
		var owner = Mapper.Map<OwnerEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
		};

		await _repository.AddAsync(owner);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}

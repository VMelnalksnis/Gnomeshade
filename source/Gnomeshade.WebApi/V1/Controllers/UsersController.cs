// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Authentication;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on user entity.</summary>
public sealed class UsersController : FinanceControllerBase<UserEntity, UserModel>
{
	private readonly UserRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="UsersController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="UserEntity"/>.</param>
	public UsersController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		UserRepository repository)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetUsersAsync"/>
	/// <response code="200">Successfully got all units.</response>
	[HttpGet]
	public async Task<IEnumerable<UserModel>> Get(CancellationToken cancellationToken)
	{
		var entities = await _repository.Get(cancellationToken);
		return entities.Select(MapToModel);
	}
}

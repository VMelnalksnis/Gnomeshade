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
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Owners;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Owners;

/// <summary>CRUD operations on <see cref="Owner"/>.</summary>
public sealed class OwnersController : FinanceControllerBase<OwnerEntity, Owner>
{
	private readonly OwnerRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="OwnersController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="OwnerEntity"/>.</param>
	public OwnersController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<OwnersController> logger,
		OwnerRepository repository)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetOwnersAsync"/>
	[HttpGet]
	[ProducesResponseType(typeof(List<Owner>), Status200OK)]
	public async Task<List<Owner>> Get(CancellationToken cancellationToken)
	{
		var owners = await _repository.GetAllAsync(cancellationToken);
		return owners.Select(MapToModel).ToList();
	}
}

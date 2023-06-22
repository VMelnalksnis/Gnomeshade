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
using Gnomeshade.WebApi.Models.Owners;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on access types.</summary>
public sealed class AccessController : FinanceControllerBase<AccessEntity, Access>
{
	private readonly AccessRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="AccessController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="AccessEntity"/>.</param>
	public AccessController(Mapper mapper, AccessRepository repository)
		: base(mapper)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IOwnerClient.GetAccessesAsync"/>
	/// <response code="200">Successfully got all accesses.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Access>), Status200OK)]
	public async Task<List<Access>> Get(CancellationToken cancellationToken)
	{
		var accesses = await _repository.GetAllAsync(cancellationToken);
		return accesses.Select(MapToModel).ToList();
	}
}

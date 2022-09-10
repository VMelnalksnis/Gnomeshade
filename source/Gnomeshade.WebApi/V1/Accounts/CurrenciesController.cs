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
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Accounts;

/// <summary>CRUD operations on currency entity.</summary>
public sealed class CurrenciesController : FinanceControllerBase<CurrencyEntity, Currency>
{
	private readonly CurrencyRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="CurrenciesController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="CurrencyEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public CurrenciesController(
		CurrencyRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<CurrenciesController> logger)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="IAccountClient.GetCurrenciesAsync"/>
	/// <response code="200">Successfully got all currencies.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Currency>), Status200OK)]
	public async Task<ActionResult<List<Currency>>> GetCurrencies(CancellationToken cancellationToken)
	{
		var currencies = await _repository.GetAllAsync(cancellationToken);
		var models = currencies.Select(currency => MapToModel(currency)).ToList();
		return Ok(models);
	}
}

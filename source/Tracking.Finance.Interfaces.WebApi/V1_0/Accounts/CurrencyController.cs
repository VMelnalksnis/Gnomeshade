// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	/// <summary>
	/// CRUD operations on currency entity.
	/// </summary>
	[ApiController]
	[ApiVersion("1.0")]
	[Authorize]
	[Route("api/v{version:apiVersion}/[controller]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public class CurrencyController : ControllerBase
	{
		private readonly CurrencyRepository _currencyRepository;
		private readonly Mapper _mapper;

		public CurrencyController(CurrencyRepository currencyRepository, Mapper mapper)
		{
			_currencyRepository = currencyRepository;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<CurrencyModel>>> GetCurrencies(CancellationToken cancellationToken)
		{
			var currencies = await _currencyRepository.GetAllAsync(cancellationToken);
			var models = currencies.Select(currency => _mapper.Map<CurrencyModel>(currency));
			return Ok(models);
		}
	}
}

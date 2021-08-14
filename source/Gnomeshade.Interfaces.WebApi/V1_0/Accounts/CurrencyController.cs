// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts
{
	/// <summary>
	/// CRUD operations on currency entity.
	/// </summary>
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class CurrencyController : FinanceControllerBase<Currency, CurrencyModel>
	{
		private readonly CurrencyRepository _currencyRepository;

		public CurrencyController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			CurrencyRepository currencyRepository,
			Mapper mapper)
			: base(userManager, userRepository, mapper)
		{
			_currencyRepository = currencyRepository;
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<CurrencyModel>>> GetCurrencies(CancellationToken cancellationToken)
		{
			var currencies = await _currencyRepository.GetAllAsync(cancellationToken);
			var models = currencies.Select(currency => MapToModel(currency)).ToList();
			return Ok(models);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_currencyRepository.Dispose();
			base.Dispose(disposing);
		}
	}
}

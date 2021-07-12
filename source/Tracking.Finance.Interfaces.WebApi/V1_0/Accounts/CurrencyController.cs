// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Interfaces.WebApi.Helpers;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
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
		private readonly Mapper _mapper;

		public CurrencyController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			CurrencyRepository currencyRepository,
			Mapper mapper)
			: base(userManager, userRepository)
		{
			_currencyRepository = currencyRepository;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<CurrencyModel>>> GetCurrencies(CancellationToken cancellationToken)
		{
			var currencies = await _currencyRepository.GetAllAsync(cancellationToken);
			var models = (await currencies.SelectAsync(currency => GetModel(currency, cancellationToken))).ToList();
			return Ok(models);
		}

		/// <inheritdoc />
		protected override Task<CurrencyModel> GetModel(Currency entity, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<CurrencyModel>(cancellationToken);
			}

			var model = _mapper.Map<CurrencyModel>(entity);
			return Task.FromResult(model);
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

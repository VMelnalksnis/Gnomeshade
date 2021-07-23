// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	/// <summary>
	/// CRUD operations on account entity.
	/// </summary>
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class AccountController : FinanceControllerBase<Account, AccountModel>
	{
		private readonly IDbConnection _dbConnection;
		private readonly AccountRepository _repository;
		private readonly AccountInCurrencyRepository _inCurrencyRepository;
		private readonly CurrencyRepository _currencyRepository;
		private readonly ILogger<AccountController> _logger;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			IDbConnection dbConnection,
			AccountRepository repository,
			AccountInCurrencyRepository inCurrencyRepository,
			CurrencyRepository currencyRepository,
			Mapper mapper,
			ILogger<AccountController> logger)
			: base(userManager, userRepository, mapper)
		{
			_dbConnection = dbConnection;
			_repository = repository;
			_inCurrencyRepository = inCurrencyRepository;
			_currencyRepository = currencyRepository;
			_logger = logger;
		}

		[HttpGet("find/{name}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public Task<ActionResult<AccountModel>> Find(string name, CancellationToken cancellation)
		{
			return Find(() => _repository.FindByNameAsync(name.ToUpperInvariant(), cancellation));
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public Task<ActionResult<AccountModel>> Get(Guid id, CancellationToken cancellation)
		{
			return Find(() => _repository.FindByIdAsync(id, cancellation));
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<AccountModel>>> GetAll(CancellationToken cancellation)
		{
			var accounts = await _repository.GetAllAsync(cancellation);
			var models = accounts.Select(MapToModel).ToList();
			return Ok(models);
		}

		[HttpPost]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] AccountCreationModel creationModel)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			var account = Mapper.Map<Account>(creationModel) with
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				NormalizedName = creationModel.Name!.ToUpperInvariant(),
			};

			_dbConnection.Open();
			using var dbTransaction = _dbConnection.BeginTransaction();

			try
			{
				var id = await _repository.AddAsync(account, dbTransaction);
				var inCurrencies =
					creationModel
						.Currencies!
						.Select(currency => Mapper.Map<AccountInCurrency>(currency) with
						{
							OwnerId = user.Id,
							CreatedByUserId = user.Id,
							ModifiedByUserId = user.Id,
							AccountId = id,
						});

				foreach (var accountInCurrency in inCurrencies)
				{
					_ = await _inCurrencyRepository.AddAsync(accountInCurrency, dbTransaction);
				}

				dbTransaction.Commit();
				return CreatedAtAction(nameof(Get), new { id }, id);
			}
			catch (Exception exception)
			{
				_logger.LogWarning(exception, "Failed to create account");
				dbTransaction.Rollback();
				throw;
			}
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_dbConnection.Dispose();
			_repository.Dispose();
			_inCurrencyRepository.Dispose();
			_currencyRepository.Dispose();
			base.Dispose(disposing);
		}
	}
}

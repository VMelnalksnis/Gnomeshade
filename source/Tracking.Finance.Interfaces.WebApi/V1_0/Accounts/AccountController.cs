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

using Microsoft.AspNetCore.Authorization;
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
	[ApiController]
	[ApiVersion("1.0")]
	[Authorize]
	[Route("api/v{version:apiVersion}/[controller]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class AccountController : ControllerBase, IDisposable
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;
		private readonly IDbConnection _dbConnection;
		private readonly AccountRepository _repository;
		private readonly AccountInCurrencyRepository _inCurrencyRepository;
		private readonly CurrencyRepository _currencyRepository;
		private readonly Mapper _mapper;
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
		{
			_userManager = userManager;
			_userRepository = userRepository;
			_dbConnection = dbConnection;
			_repository = repository;
			_inCurrencyRepository = inCurrencyRepository;
			_currencyRepository = currencyRepository;
			_mapper = mapper;
			_logger = logger;
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<ActionResult<AccountModel>> Get(Guid id, CancellationToken cancellationToken)
		{
			var account = await _repository.FindByIdAsync(id, cancellationToken);
			if (account is null)
			{
				return NotFound();
			}

			var preferredCurrency =
				await _currencyRepository.GetByIdAsync(account.PreferredCurrencyId, cancellationToken);
			var inCurrencies = await _inCurrencyRepository.GetByAccountIdAsync(account.Id, cancellationToken);
			var models =
				inCurrencies
					.Select(inCurrency =>
					{
						var currency =
							_currencyRepository
								.GetByIdAsync(inCurrency.CurrencyId, cancellationToken)
								.GetAwaiter()
								.GetResult();

						return _mapper.Map<AccountInCurrencyModel>(inCurrency) with
						{
							Currency = _mapper.Map<CurrencyModel>(currency),
						};
					})
					.ToList();

			return Ok(_mapper.Map<AccountModel>(account) with
			{
				PreferredCurrency = _mapper.Map<CurrencyModel>(preferredCurrency),
				Currencies = models,
			});
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<AccountModel>>> GetAll(CancellationToken cancellationToken)
		{
			var accounts = await _repository.GetAllAsync(cancellationToken);

			var models =
				accounts
					.Select(account =>
					{
						var preferredCurrency =
							_currencyRepository
								.GetByIdAsync(account.PreferredCurrencyId, cancellationToken)
								.GetAwaiter()
								.GetResult();

						var inCurrencies =
							_inCurrencyRepository
								.GetByAccountIdAsync(account.Id, cancellationToken)
								.GetAwaiter()
								.GetResult();

						var currencyModels =
							inCurrencies
								.Select(accountInCurrency =>
								{
									var currency =
										_currencyRepository
											.GetByIdAsync(accountInCurrency.CurrencyId, cancellationToken)
											.GetAwaiter()
											.GetResult();

									return _mapper.Map<AccountInCurrencyModel>(accountInCurrency) with
									{
										Currency = _mapper.Map<CurrencyModel>(currency),
									};
								})
								.ToList();

						return _mapper.Map<AccountModel>(account) with
						{
							PreferredCurrency = _mapper.Map<CurrencyModel>(preferredCurrency),
							Currencies = currencyModels,
						};
					})
					.ToList();

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

			var account = _mapper.Map<Account>(creationModel) with
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
						.Select(currency => _mapper.Map<AccountInCurrency>(currency) with
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
		public void Dispose()
		{
			_userManager.Dispose();
			_userRepository.Dispose();
			_dbConnection.Dispose();
			_repository.Dispose();
			_inCurrencyRepository.Dispose();
			_currencyRepository.Dispose();
		}

		private async Task<User?> GetCurrentUser()
		{
			var identityUser = await _userManager.GetUserAsync(User);
			if (identityUser is null)
			{
				return null;
			}

			return await _userRepository.FindByIdAsync(new(identityUser.Id));
		}
	}
}

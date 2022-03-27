// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

/// <summary>CRUD operations on account entity.</summary>
public sealed class AccountsController : FinanceControllerBase<AccountEntity, Account>
{
	private readonly AccountRepository _repository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly AccountUnitOfWork _accountUnitOfWork;

	/// <summary>Initializes a new instance of the <see cref="AccountsController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="inCurrencyRepository">The repository for performing CRUD operations on <see cref="AccountInCurrencyEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="accountUnitOfWork">Unit of work for managing accounts and all related entities.</param>
	public AccountsController(
		AccountRepository repository,
		AccountInCurrencyRepository inCurrencyRepository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<AccountsController> logger,
		AccountUnitOfWork accountUnitOfWork)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
		_inCurrencyRepository = inCurrencyRepository;
		_accountUnitOfWork = accountUnitOfWork;
	}

	/// <inheritdoc cref="IAccountClient.GetAccountAsync"/>
	/// <response code="200">Successfully got the account.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Account>> Get(Guid id, CancellationToken cancellation)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellation));
	}

	/// <summary>Gets all accounts.</summary>
	/// <param name="onlyActive">Whether to get only active accounts.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all accounts.</returns>
	/// <response code="200">Successfully got all accounts.</response>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<Account>>> GetAll(
		[FromQuery, DefaultValue(true)] bool onlyActive,
		CancellationToken cancellationToken)
	{
		var accounts = onlyActive
			? await _repository.GetAllActiveAsync(ApplicationUser.Id, cancellationToken)
			: await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);

		// ReSharper disable once ConvertClosureToMethodGroup
		var models = accounts.Select(account => MapToModel(account)).ToList();
		return Ok(models);
	}

	/// <inheritdoc cref="IAccountClient.CreateAccountAsync"/>
	/// <response code="201">A new account was created.</response>
	/// <response code="409">An account with the specified name already exists.</response>
	[HttpPost]
	[ProducesResponseType(typeof(Guid), Status201Created)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Post([FromBody] AccountCreationModel account)
	{
		var conflictingResult = await GetConflictResult(account, ApplicationUser);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var entity = Mapper.Map<AccountEntity>(account) with
		{
			OwnerId = ApplicationUser.Id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			NormalizedName = account.Name!.ToUpperInvariant(),
		};

		var id = await _accountUnitOfWork.AddAsync(entity);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	/// <inheritdoc cref="IAccountClient.PutAccountAsync"/>
	/// <response code="201">A new account was created.</response>
	/// <response code="204">An existing account was replaced.</response>
	/// <response code="409">An account with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody] AccountCreationModel account)
	{
		var existingAccount = await _repository.FindByIdAsync(id, ApplicationUser.Id);

		return existingAccount is null
			? await PutNewAccountAsync(account, ApplicationUser, id)
			: await UpdateExistingAccountAsync(account, ApplicationUser, existingAccount);
	}

	/// <inheritdoc cref="IAccountClient.AddCurrencyToAccountAsync"/>
	/// <response code="201">Currency was successfully added.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	/// <response code="409">The account already has the specified currency.</response>
	[HttpPost("{id:guid}/Currencies")]
	[ProducesResponseType(Status201Created)]
	[ProducesStatus404NotFound]
	[ProducesStatus409Conflict]
	public async Task<ActionResult<Guid>> AddCurrency(Guid id, [FromBody] AccountInCurrencyCreationModel currency)
	{
		var account = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		if (account is null)
		{
			return NotFound();
		}

		var conflictingCurrency = account.Currencies.FirstOrDefault(c => c.CurrencyId == currency.CurrencyId);
		if (conflictingCurrency is not null)
		{
			return Problem(
				"The account already has the specified currency",
				Url.Action(nameof(Get), new { conflictingCurrency.AccountId }),
				Status409Conflict);
		}

		var accountInCurrency = Mapper.Map<AccountInCurrencyEntity>(currency) with
		{
			OwnerId = account.OwnerId,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			AccountId = account.Id,
		};

		_ = await _inCurrencyRepository.AddAsync(accountInCurrency);

		// todo should this point to account or account in currency?
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult> PutNewAccountAsync(AccountCreationModel model, UserEntity user, Guid id)
	{
		var conflictingResult = await GetConflictResult(model, user);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var account = Mapper.Map<AccountEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = model.Name!.ToUpperInvariant(),
		};

		_ = await _accountUnitOfWork.AddAsync(account);
		return CreatedAtAction(nameof(Get), new { id }, string.Empty);
	}

	private async Task<ActionResult> UpdateExistingAccountAsync(
		AccountCreationModel model,
		UserEntity user,
		AccountEntity existingAccount)
	{
		var conflictingResult = await GetConflictResult(model, user, existingAccount.Id);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var account = Mapper.Map<AccountEntity>(model) with
		{
			Id = existingAccount.Id,
			OwnerId = user.Id, // todo only works for entities created by the user
			NormalizedName = model.Name!.ToUpperInvariant(),
		};

		_ = await _accountUnitOfWork.UpdateAsync(account, user);
		return NoContent();
	}

	private async Task<ActionResult?> GetConflictResult(
		AccountCreationModel model,
		UserEntity user,
		Guid? existingAccountId = null)
	{
		var normalizedName = model.Name!.ToUpperInvariant();
		var conflictingAccount = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingAccount is null ||
			conflictingAccount.Id == existingAccountId ||
			conflictingAccount.CounterpartyId != model.CounterpartyId)
		{
			return null;
		}

		return Problem(
			"Account with the specified name already exists",
			Url.Action(nameof(Get), new { conflictingAccount.Id }),
			Status409Conflict);
	}
}

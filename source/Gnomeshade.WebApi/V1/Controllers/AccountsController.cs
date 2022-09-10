// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on account entity.</summary>
public sealed class AccountsController : CreatableBase<AccountRepository, AccountEntity, Account, AccountCreation>
{
	private readonly AccountRepository _repository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly AccountUnitOfWork _accountUnitOfWork;

	/// <summary>Initializes a new instance of the <see cref="AccountsController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="inCurrencyRepository">The repository for performing CRUD operations on <see cref="AccountInCurrencyEntity"/>.</param>
	/// <param name="accountUnitOfWork">Unit of work for managing accounts and all related entities.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public AccountsController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<AccountsController> logger,
		AccountRepository repository,
		AccountInCurrencyRepository inCurrencyRepository,
		AccountUnitOfWork accountUnitOfWork,
		DbConnection dbConnection)
		: base(applicationUserContext, mapper, logger, repository, dbConnection)
	{
		_repository = repository;
		_inCurrencyRepository = inCurrencyRepository;
		_accountUnitOfWork = accountUnitOfWork;
	}

	/// <inheritdoc />
	[NonAction]
	public override Task<List<Account>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <summary>Gets all accounts.</summary>
	/// <param name="onlyActive">Whether to get only active accounts.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all accounts.</returns>
	/// <response code="200">Successfully got all accounts.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Account>), Status200OK)]
	public async Task<List<Account>> GetAll(
		[FromQuery, DefaultValue(true)] bool onlyActive,
		CancellationToken cancellationToken)
	{
		var accounts = onlyActive
			? await _repository.GetAllActiveAsync(ApplicationUser.Id, cancellationToken)
			: await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);

		return accounts.Select(MapToModel).ToList();
	}

	/// <inheritdoc cref="IAccountClient.GetAccountAsync"/>
	/// <response code="200">Successfully got the account.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Account), Status200OK)]
	public override Task<ActionResult<Account>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IAccountClient.CreateAccountAsync"/>
	/// <response code="201">A new account was created.</response>
	/// <response code="409">An account with the specified name already exists.</response>
	public override Task<ActionResult> Post([FromBody] AccountCreation account) =>
		base.Post(account);

	/// <inheritdoc cref="IAccountClient.PutAccountAsync"/>
	/// <response code="201">A new account was created.</response>
	/// <response code="204">An existing account was replaced.</response>
	/// <response code="403">An account with the specified id already exists, but you do not have access to it.</response>
	/// <response code="409">An account with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] AccountCreation account) =>
		base.Put(id, account);

	/// <inheritdoc cref="IAccountClient.AddCurrencyToAccountAsync"/>
	/// <response code="201">Currency was successfully added.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	/// <response code="409">The account already has the specified currency.</response>
	[HttpPost("{id:guid}/Currencies")]
	[ProducesResponseType(Status201Created)]
	[ProducesStatus404NotFound]
	[ProducesStatus409Conflict]
	public async Task<ActionResult<Guid>> AddCurrency(Guid id, [FromBody] AccountInCurrencyCreation currency)
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

	/// <inheritdoc cref="IAccountClient.RemoveCurrencyFromAccountAsync"/>
	/// <response code="204">Currency was successfully removed from account.</response>
	/// <response code="404">Account or currency with the specified id does not exist.</response>
	[HttpDelete("{id:guid}/Currencies/{currencyId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> RemoveCurrency(Guid id, Guid currencyId)
	{
		var account = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		if (account is null)
		{
			return NotFound();
		}

		var currency = await _inCurrencyRepository.FindByIdAsync(currencyId, ApplicationUser.Id);
		if (currency is null)
		{
			return NotFound();
		}

		await _inCurrencyRepository.DeleteAsync(currencyId, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="IAccountClient.GetAccountBalanceAsync"/>
	[HttpGet("{id:guid}/Balance")]
	public async Task<ActionResult<List<Balance>>> Balance(Guid id, CancellationToken cancellationToken)
	{
		var account = await _repository.FindByIdAsync(id, ApplicationUser.Id, AccessLevel.Read, cancellationToken);
		if (account is null)
		{
			return NotFound();
		}

		var entities = await _repository.GetBalanceAsync(id, ApplicationUser.Id, cancellationToken);
		var models = entities.Select(entity => Mapper.Map<Balance>(entity)).ToList();
		return Ok(models);
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, AccountCreation creation, UserEntity user)
	{
		var conflictingResult = await GetConflictResult(creation, user, id);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var account = Mapper.Map<AccountEntity>(creation) with { Id = id };

		await _accountUnitOfWork.UpdateAsync(account, user);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, AccountCreation creation, UserEntity user)
	{
		var conflictingResult = await GetConflictResult(creation, user);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var account = Mapper.Map<AccountEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
		};

		_ = await _accountUnitOfWork.AddAsync(account);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult?> GetConflictResult(
		AccountCreation model,
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

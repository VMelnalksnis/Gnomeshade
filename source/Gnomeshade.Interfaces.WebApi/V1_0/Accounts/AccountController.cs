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
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

/// <summary>
/// CRUD operations on account entity.
/// </summary>
public sealed class AccountController : FinanceControllerBase<AccountEntity, Account>
{
	private readonly AccountRepository _repository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly AccountUnitOfWork _accountUnitOfWork;

	/// <summary>
	/// Initializes a new instance of the <see cref="AccountController"/> class.
	/// </summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="inCurrencyRepository">The repository for performing CRUD operations on <see cref="AccountInCurrencyEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="accountUnitOfWork">Unit of work for managing accounts and all related entities.</param>
	public AccountController(
		AccountRepository repository,
		AccountInCurrencyRepository inCurrencyRepository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		AccountUnitOfWork accountUnitOfWork)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
		_inCurrencyRepository = inCurrencyRepository;
		_accountUnitOfWork = accountUnitOfWork;
	}

	/// <summary>
	/// Gets the account with the specified name.
	/// </summary>
	/// <param name="name">The name of the account.</param>
	/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The account with the specified name.</returns>
	/// <response code="200">Successfully got the account.</response>
	/// <response code="404">Account with the specified name does not exist.</response>
	[HttpGet("find/{name}")] // todo route
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Account>> Find(string name, CancellationToken cancellation)
	{
		return await Find(() => _repository.FindByNameAsync(name.ToUpperInvariant(), ApplicationUser.Id, cancellation));
	}

	/// <summary>
	/// Gets the account with the specified id.
	/// </summary>
	/// <param name="id">The id of the account.</param>
	/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The account with the specified id.</returns>
	/// <response code="200">Successfully got the account.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	[HttpGet("{id:guid}", Name = "GetAccountById")]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Account>> Get(Guid id, CancellationToken cancellation)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellation));
	}

	/// <summary>
	/// Gets all accounts.
	/// </summary>
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

	/// <summary>
	/// Creates a new account.
	/// </summary>
	/// <param name="model">The account to create.</param>
	/// <returns>The id of the created account.</returns>
	/// <response code="201">A new account was created.</response>
	/// <response code="409">An account with the specified name already exists.</response>
	[HttpPost]
	[ProducesResponseType(typeof(Guid), Status201Created)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Post([FromBody, BindRequired] AccountCreationModel model)
	{
		var conflictingResult = await GetConflictResult(model, ApplicationUser);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var account = Mapper.Map<AccountEntity>(model) with
		{
			OwnerId = ApplicationUser.Id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			NormalizedName = model.Name!.ToUpperInvariant(),
		};

		var id = await _accountUnitOfWork.AddAsync(account);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	/// <summary>
	/// Creates a new account, or replaces an existing one with the specified id.
	/// </summary>
	/// <param name="id">The id of the account.</param>
	/// <param name="model">The account to create or replace.</param>
	/// <returns>A status code indicating the result of the action.</returns>
	/// <response code="201">A new account was created.</response>
	/// <response code="204">An existing account was replaced.</response>
	/// <response code="409">An account with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody, BindRequired] AccountCreationModel model)
	{
		var existingAccount = await _repository.FindByIdAsync(id, ApplicationUser.Id);

		return existingAccount is null
			? await PutNewAccountAsync(model, ApplicationUser, id)
			: await UpdateExistingAccountAsync(model, ApplicationUser, existingAccount);
	}

	/// <summary>
	/// Add a new currency to an existing account.
	/// </summary>
	/// <param name="id">The id of the account to which to add the currency.</param>
	/// <param name="creationModel">The currency to add to the account.</param>
	/// <returns>The id of the account to which the currency was added to.</returns>
	/// <response code="201">Currency was successfully added.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	/// <response code="409">The account already has the specified currency.</response>
	[HttpPost("{id:guid}/Currency")]
	[ProducesResponseType(Status201Created)]
	[ProducesStatus404NotFound]
	[ProducesStatus409Conflict]
	public async Task<ActionResult<Guid>> AddCurrency(
		Guid id,
		[FromBody, BindRequired] AccountInCurrencyCreationModel creationModel)
	{
		var account = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		if (account is null)
		{
			return NotFound();
		}

		if (account.Currencies.Any(currency => currency.CurrencyId == creationModel.CurrencyId))
		{
			return Problem(
				"The account already has the specified currency",
				Url.Action(nameof(Get), new { id }), // todo link to currency instead
				Status409Conflict);
		}

		var accountInCurrency = Mapper.Map<AccountInCurrencyEntity>(creationModel) with
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

﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

/// <summary>
/// CRUD operations on account entity.
/// </summary>
[SuppressMessage(
	"ReSharper",
	"AsyncConverter.ConfigureAwaitHighlighting",
	Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
public sealed class AccountController : FinanceControllerBase<AccountEntity, Account>
{
	private readonly AccountRepository _repository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly AccountUnitOfWork _accountUnitOfWork;

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

	[HttpGet("find/{name}")]
	[ProducesResponseType(Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
	public Task<ActionResult<Account>> Find(string name, CancellationToken cancellation)
	{
		return Find(() => _repository.FindByNameAsync(name.ToUpperInvariant(), ApplicationUser.Id, cancellation));
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
	public Task<ActionResult<Account>> Get(Guid id, CancellationToken cancellation)
	{
		return Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellation));
	}

	/// <summary>
	/// Gets all accounts.
	/// </summary>
	/// <param name="onlyActive">Whether to get only active accounts.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all accounts.</returns>
	/// <response code="200">Successfully got all accounts.</response>
	[HttpGet]
	[ProducesResponseType(Status200OK)]
	public async Task<ActionResult<IEnumerable<Account>>> GetAll(
		[FromQuery, DefaultValue(true)] bool onlyActive,
		CancellationToken cancellationToken)
	{
		var accounts = onlyActive
			? await _repository.GetAllActiveAsync(ApplicationUser.Id, cancellationToken)
			: await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);

		var models = accounts.Select(account => MapToModel(account)).ToList();
		return Ok(models);
	}

	[HttpPost]
	[ProducesResponseType(Status201Created)]
	public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] AccountCreationModel creationModel)
	{
		var account = Mapper.Map<AccountEntity>(creationModel) with
		{
			OwnerId = ApplicationUser.Id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			NormalizedName = creationModel.Name!.ToUpperInvariant(),
		};

		var id = await _accountUnitOfWork.AddAsync(account);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	/// <summary>
	/// Add a new currency to an existing account.
	/// </summary>
	/// <param name="id">The id of the account to which to add the currency.</param>
	/// <param name="creationModel">The currency to add to the account.</param>
	/// <returns>The id of the account to which the currency was added to.</returns>
	/// <response code="201">Currency was successfully added.</response>
	/// <response code="404">Account with the specified id does not exist.</response>
	[HttpPost("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status404NotFound)]
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
			// todo return full bad request response, instead of just the error dictionary
			ModelState.AddModelError(nameof(creationModel.CurrencyId), "The currency already exists.");
			return BadRequest(ModelState);
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
}

﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

using JetBrains.Annotations;

namespace Gnomeshade.Data;

/// <summary>Performs bulk actions on account entities.</summary>
public sealed class AccountUnitOfWork
{
	private readonly AccountRepository _repository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly CounterpartyRepository _counterpartyRepository;

	/// <summary>Initializes a new instance of the <see cref="AccountUnitOfWork"/> class.</summary>
	/// <param name="repository">The repository for managing accounts.</param>
	/// <param name="inCurrencyRepository">The repository for managing accounts in currencies.</param>
	/// <param name="counterpartyRepository">The repository for managing counterparties.</param>
	public AccountUnitOfWork(
		AccountRepository repository,
		AccountInCurrencyRepository inCurrencyRepository,
		CounterpartyRepository counterpartyRepository)
	{
		_repository = repository;
		_inCurrencyRepository = inCurrencyRepository;
		_counterpartyRepository = counterpartyRepository;
	}

	/// <summary>Creates a new account with the currencies in <see cref="AccountEntity.Currencies"/>.</summary>
	/// <param name="account">The account to create.</param>
	/// <param name="dbTransaction">The database transaction to use for queries.</param>
	/// <returns>The id of the created account.</returns>
	public async Task<Guid> AddAsync(AccountEntity account, DbTransaction dbTransaction)
	{
		if (account.Id == Guid.Empty)
		{
			account = account with { Id = Guid.NewGuid() };
		}

		var id = await _repository.AddAsync(account, dbTransaction).ConfigureAwait(false);

		foreach (var currencyId in account.Currencies.Select(inCurrency => inCurrency.CurrencyId))
		{
			var inCurrency = new AccountInCurrencyEntity
			{
				Id = Guid.NewGuid(),
				OwnerId = account.OwnerId,
				CreatedByUserId = account.CreatedByUserId,
				ModifiedByUserId = account.ModifiedByUserId,
				AccountId = id,
				CurrencyId = currencyId,
			};

			_ = await _inCurrencyRepository.AddAsync(inCurrency, dbTransaction);
		}

		return id;
	}

	/// <summary>Creates a new account with the currencies in <see cref="AccountEntity.Currencies"/> and a counterparty.</summary>
	/// <param name="account">The account to create.</param>
	/// <param name="dbTransaction">The database transaction to use for queries.</param>
	/// <returns>The id of the created account.</returns>
	public async Task<Guid> AddWithCounterpartyAsync(AccountEntity account, DbTransaction dbTransaction)
	{
		var counterparty = new CounterpartyEntity
		{
			Id = Guid.NewGuid(),
			OwnerId = account.OwnerId,
			CreatedByUserId = account.CreatedByUserId,
			ModifiedByUserId = account.ModifiedByUserId,
			Name = account.Name,
		};

		var counterpartyId = await _counterpartyRepository.AddAsync(counterparty, dbTransaction);
		account.CounterpartyId = counterpartyId;

		return await AddAsync(account, dbTransaction);
	}

	/// <summary>Deletes the specified account and all its currencies.</summary>
	/// <param name="account">The account to delete.</param>
	/// <param name="userId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for queries.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteAsync(AccountEntity account, Guid userId, DbTransaction dbTransaction)
	{
		foreach (var currency in account.Currencies)
		{
			if (await _inCurrencyRepository.DeleteAsync(currency.Id, userId, dbTransaction) is not 1)
			{
				throw new InvalidOperationException("Failed to delete account in currency");
			}
		}

		if (await _repository.DeleteAsync(account.Id, userId, dbTransaction) is not 1)
		{
			throw new InvalidOperationException("Failed to delete account");
		}
	}

	/// <summary>Updates the specified account.</summary>
	/// <param name="account">The account to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="account"/>.</param>
	/// <param name="dbTransaction">The database transaction to use for queries.</param>
	/// <returns>The number of affected rows.</returns>
	[MustUseReturnValue]
	public async Task<int> UpdateAsync(AccountEntity account, UserEntity modifiedBy, DbTransaction dbTransaction)
	{
		account.ModifiedByUserId = modifiedBy.Id;
		return await _repository.UpdateAsync(account, dbTransaction);
	}
}

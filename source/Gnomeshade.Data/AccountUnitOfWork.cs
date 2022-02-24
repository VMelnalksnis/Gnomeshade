// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data;

/// <summary>Performs bulk actions on account entities.</summary>
public sealed class AccountUnitOfWork : IDisposable
{
	private readonly IDbConnection _dbConnection;
	private readonly AccountRepository _repository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly CounterpartyRepository _counterpartyRepository;

	/// <summary>Initializes a new instance of the <see cref="AccountUnitOfWork"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	/// <param name="repository">The repository for managing accounts.</param>
	/// <param name="inCurrencyRepository">The repository for managing accounts in currencies.</param>
	/// <param name="counterpartyRepository">The repository for managing counterparties.</param>
	public AccountUnitOfWork(
		IDbConnection dbConnection,
		AccountRepository repository,
		AccountInCurrencyRepository inCurrencyRepository,
		CounterpartyRepository counterpartyRepository)
	{
		_dbConnection = dbConnection;
		_repository = repository;
		_inCurrencyRepository = inCurrencyRepository;
		_counterpartyRepository = counterpartyRepository;
	}

	/// <summary>Creates a new account with the currencies in <see cref="AccountEntity.Currencies"/>.</summary>
	/// <param name="account">The account to create.</param>
	/// <returns>The id of the created account.</returns>
	public async Task<Guid> AddAsync(AccountEntity account)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

		try
		{
			var id = await AddAsync(account, dbTransaction).ConfigureAwait(false);
			dbTransaction.Commit();
			return id;
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>Creates a new account with the currencies in <see cref="AccountEntity.Currencies"/>.</summary>
	/// <param name="account">The account to create.</param>
	/// <param name="dbTransaction">The database transaction to use for queries.</param>
	/// <returns>The id of the created account.</returns>
	public async Task<Guid> AddAsync(AccountEntity account, IDbTransaction dbTransaction)
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
	public async Task<Guid> AddWithCounterpartyAsync(AccountEntity account, IDbTransaction dbTransaction)
	{
		var counterparty = new CounterpartyEntity
		{
			Id = Guid.NewGuid(),
			OwnerId = account.OwnerId,
			CreatedByUserId = account.CreatedByUserId,
			ModifiedByUserId = account.ModifiedByUserId,
			Name = account.Name,
			NormalizedName = account.NormalizedName,
		};

		var counterpartyId = await _counterpartyRepository.AddAsync(counterparty, dbTransaction);
		account.CounterpartyId = counterpartyId;

		return await AddAsync(account, dbTransaction);
	}

	/// <summary>Deletes the specified account and all its currencies.</summary>
	/// <param name="account">The account to delete.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <returns>The number of affected rows.</returns>
	public async Task<int> DeleteAsync(AccountEntity account, Guid ownerId)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

		try
		{
			var rows = await DeleteAsync(account, ownerId, dbTransaction).ConfigureAwait(false);
			dbTransaction.Commit();
			return rows;
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <summary>Updates the specified account.</summary>
	/// <param name="account">The account to update.</param>
	/// <param name="modifiedBy">The user which modified the <paramref name="account"/>.</param>
	/// <returns>The number of affected rows.</returns>
	public async Task<int> UpdateAsync(AccountEntity account, UserEntity modifiedBy)
	{
		using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

		try
		{
			var rows = await UpdateAsync(account, modifiedBy, dbTransaction);
			dbTransaction.Commit();
			return rows;
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_dbConnection.Dispose();
		_repository.Dispose();
		_inCurrencyRepository.Dispose();
		_counterpartyRepository.Dispose();
	}

	private async Task<int> DeleteAsync(AccountEntity account, Guid ownerId, IDbTransaction dbTransaction)
	{
		var rows = 0;
		foreach (var currency in account.Currencies)
		{
			rows += await _inCurrencyRepository.DeleteAsync(currency.Id, ownerId, dbTransaction).ConfigureAwait(false);
		}

		rows += await _repository.DeleteAsync(account.Id, ownerId, dbTransaction).ConfigureAwait(false);

		Debug.Assert(rows > 0, "No rows were modified when deleting an account.");
		return rows;
	}

	private async Task<int> UpdateAsync(AccountEntity account, UserEntity modifiedBy, IDbTransaction dbTransaction)
	{
		account.ModifiedByUserId = modifiedBy.Id;
		var rows = await _repository.UpdateAsync(account, dbTransaction).ConfigureAwait(false);

		Debug.Assert(rows > 0, "No rows were modified when updating an account.");
		return rows;
	}
}

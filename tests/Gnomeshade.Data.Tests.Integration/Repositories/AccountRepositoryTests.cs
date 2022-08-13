﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using NodaTime;

using Npgsql;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public class AccountRepositoryTests : IDisposable
{
	private IDbConnection _dbConnection = null!;
	private CounterpartyRepository _counterpartyRepository = null!;
	private AccountRepository _repository = null!;
	private AccountInCurrencyRepository _inCurrencyRepository = null!;
	private AccountUnitOfWork _unitOfWork = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_counterpartyRepository = new(_dbConnection);
		_repository = new(_dbConnection);
		_inCurrencyRepository = new(_dbConnection);
		_unitOfWork = new(_dbConnection, _repository, _inCurrencyRepository, new(_dbConnection));
	}

	[TearDown]
	public void Dispose()
	{
		_dbConnection.Dispose();
		_counterpartyRepository.Dispose();
		_repository.Dispose();
		_inCurrencyRepository.Dispose();
		_unitOfWork.Dispose();
	}

	[Test]
	public async Task AddAsync_WithTransaction()
	{
		var currencies = await EntityFactory.GetCurrenciesAsync();
		var preferredCurrency = currencies.First();

		var counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		var counterPartyId = await _counterpartyRepository.AddAsync(counterParty);
		counterParty = await _counterpartyRepository.GetByIdAsync(counterPartyId, TestUser.Id);

		var accountFaker = new AccountFaker(TestUser, counterParty, preferredCurrency);
		var account = accountFaker.Generate();
		foreach (var currency in currencies.Where(c => c.Id != preferredCurrency.Id))
		{
			account.Currencies.Add(new() { CurrencyId = currency.Id });
		}

		var accountId = await _unitOfWork.AddAsync(account);

		var getAccount = await _repository.GetByIdAsync(accountId, TestUser.Id);
		var findAccount = await _repository.FindByIdAsync(getAccount.Id, TestUser.Id);
		var findByNameAccount = await _repository.FindByNameAsync(getAccount.NormalizedName, TestUser.Id);
		var accounts = await _repository.GetAllAsync(TestUser.Id);

		var expectedAccount = account with
		{
			Id = accountId,
			CreatedAt = getAccount.CreatedAt,
			ModifiedAt = getAccount.ModifiedAt,
			PreferredCurrency = getAccount.PreferredCurrency,
			Currencies = getAccount.Currencies,
		};

		expectedAccount.Currencies.Should().NotBeNullOrEmpty();

		getAccount.Should().BeEquivalentTo(expectedAccount);
		findAccount.Should().BeEquivalentTo(expectedAccount);
		findByNameAccount.Should().BeEquivalentTo(expectedAccount);
		accounts.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedAccount, Options);

		var firstAccountInCurrency = getAccount.Currencies.First();
		var getAccountInCurrency = await _inCurrencyRepository.GetByIdAsync(firstAccountInCurrency.Id, TestUser.Id);
		var findAccountInCurrency = await _inCurrencyRepository.FindByIdAsync(getAccountInCurrency.Id, TestUser.Id);

		var expectedAccountInCurrency = firstAccountInCurrency with
		{
			CreatedAt = getAccountInCurrency.CreatedAt,
			ModifiedAt = getAccountInCurrency.ModifiedAt,
			Currency = getAccountInCurrency.Currency,
		};

		getAccountInCurrency.Should().BeEquivalentTo(expectedAccountInCurrency);
		findAccountInCurrency.Should().BeEquivalentTo(expectedAccountInCurrency);

		(await FluentActions.Awaiting(() => _inCurrencyRepository.AddAsync(firstAccountInCurrency))
			.Should()
			.ThrowAsync<NpgsqlException>())
			.Which.Message.Should().Contain("duplicate key value violates unique constraint");

		var disabledAccount = accountFaker.GenerateUnique(account) with
		{
			DisabledAt = SystemClock.Instance.GetCurrentInstant(),
			DisabledByUserId = TestUser.Id,
		};

		var disabledAccountId = await _unitOfWork.AddAsync(disabledAccount);

		var allAccounts = await _repository.GetAllActiveAsync(TestUser.Id);
		allAccounts.Should().OnlyContain(enabledAccount => enabledAccount.Id == getAccount.Id);

		await _unitOfWork.DeleteAsync(getAccount, TestUser.Id);
		disabledAccount = await _repository.GetByIdAsync(disabledAccountId, TestUser.Id);
		await _unitOfWork.DeleteAsync(disabledAccount, TestUser.Id);
	}

	private static EquivalencyAssertionOptions<AccountEntity> Options(
		EquivalencyAssertionOptions<AccountEntity> options)
	{
		return options
			.ComparingByMembers<AccountEntity>()
			.ComparingByMembers<AccountInCurrencyEntity>()
			.ComparingByMembers<CurrencyEntity>();
	}
}

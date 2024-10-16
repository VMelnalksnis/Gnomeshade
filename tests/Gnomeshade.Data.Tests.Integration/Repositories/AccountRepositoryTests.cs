// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using Microsoft.Extensions.Logging.Abstractions;

using Npgsql;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class AccountRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private CounterpartyRepository _counterpartyRepository = null!;
	private AccountRepository _repository = null!;
	private AccountInCurrencyRepository _inCurrencyRepository = null!;
	private AccountUnitOfWork _unitOfWork = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_counterpartyRepository = new(NullLogger<CounterpartyRepository>.Instance, _dbConnection);
		_repository = new(NullLogger<AccountRepository>.Instance, _dbConnection);
		_inCurrencyRepository = new(NullLogger<AccountInCurrencyRepository>.Instance, _dbConnection);
		_unitOfWork = new(_repository, _inCurrencyRepository, new(NullLogger<CounterpartyRepository>.Instance, _dbConnection));
	}

	[Test]
	public async Task AddAsync_WithTransaction()
	{
		var currencies = await EntityFactory.GetCurrenciesAsync();
		var preferredCurrency = currencies.First();

		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();

		var counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		var counterPartyId = await _counterpartyRepository.AddAsync(counterParty, dbTransaction);
		counterParty = await _counterpartyRepository.GetByIdAsync(counterPartyId, TestUser.Id, dbTransaction);

		var accountFaker = new AccountFaker(TestUser, counterParty, preferredCurrency);
		var account = accountFaker.Generate();
		foreach (var currency in currencies.Where(c => c.Id != preferredCurrency.Id))
		{
			account.Currencies.Add(new() { CurrencyId = currency.Id });
		}

		var accountId = await _unitOfWork.AddAsync(account, dbTransaction);

		var getAccount = await _repository.GetByIdAsync(accountId, TestUser.Id, dbTransaction);
		var findAccount = await _repository.FindByIdAsync(getAccount.Id, TestUser.Id, dbTransaction);
		var findByNameAccount = await _repository.FindByNameAsync(getAccount.NormalizedName, TestUser.Id, dbTransaction);
		var accounts = await _repository.GetAsync(TestUser.Id, dbTransaction);

		var expectedAccount = account with
		{
			Id = accountId,
			CreatedAt = getAccount.CreatedAt,
			ModifiedAt = getAccount.ModifiedAt,
			PreferredCurrencyId = getAccount.PreferredCurrencyId,
			Currencies = getAccount.Currencies,
		};

		expectedAccount.Currencies.Should().NotBeNullOrEmpty();

		getAccount.Should().BeEquivalentTo(expectedAccount);
		findAccount.Should().BeEquivalentTo(expectedAccount);
		findByNameAccount.Should().BeEquivalentTo(expectedAccount);
		accounts.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedAccount, Options);

		var firstAccountInCurrency = getAccount.Currencies.First();
		var getAccountInCurrency = await _inCurrencyRepository.GetByIdAsync(firstAccountInCurrency.Id, TestUser.Id, dbTransaction);
		var findAccountInCurrency = await _inCurrencyRepository.FindByIdAsync(getAccountInCurrency.Id, TestUser.Id, dbTransaction);

		var expectedAccountInCurrency = firstAccountInCurrency with
		{
			CreatedAt = getAccountInCurrency.CreatedAt,
			ModifiedAt = getAccountInCurrency.ModifiedAt,
			CurrencyId = getAccountInCurrency.CurrencyId,
			CurrencyAlphabeticCode = getAccountInCurrency.CurrencyAlphabeticCode,
		};

		getAccountInCurrency.Should().BeEquivalentTo(expectedAccountInCurrency);
		findAccountInCurrency.Should().BeEquivalentTo(expectedAccountInCurrency);

		(await _inCurrencyRepository.DeleteAsync(firstAccountInCurrency.Id, TestUser.Id, dbTransaction)).Should().Be(1);
		var deleted = await _inCurrencyRepository.FindByIdAsync(firstAccountInCurrency.Id, TestUser.Id, dbTransaction);
		deleted.Should().BeNull();

		await _inCurrencyRepository.RestoreDeletedAsync(getAccountInCurrency.Id, TestUser.Id, dbTransaction);

		var restored = await _inCurrencyRepository.GetByIdAsync(firstAccountInCurrency.Id, TestUser.Id, dbTransaction);
		restored.Should().BeEquivalentTo(getAccountInCurrency, options => options.Excluding(a => a.ModifiedAt));

		// ReSharper disable once AccessToDisposedClosure
		(await FluentActions.Awaiting(() => _inCurrencyRepository.AddAsync(firstAccountInCurrency, dbTransaction))
			.Should()
			.ThrowAsync<NpgsqlException>())
			.Which.Message.Should().Contain("duplicate key value violates unique constraint");
	}

	[Test]
	public async Task FindByName_ShouldIgnoreDeleted()
	{
		var currencies = await EntityFactory.GetCurrenciesAsync();
		var preferredCurrency = currencies.First();

		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();

		var counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		var counterPartyId = await _counterpartyRepository.AddAsync(counterParty, dbTransaction);
		counterParty = await _counterpartyRepository.GetByIdAsync(counterPartyId, TestUser.Id, dbTransaction);

		var accountFaker = new AccountFaker(TestUser, counterParty, preferredCurrency);
		var account = accountFaker.Generate();
		var accountId = await _unitOfWork.AddAsync(account, dbTransaction);

		var firstAccount = await _repository.GetByIdAsync(accountId, TestUser.Id, dbTransaction);
		var firstAccountName = firstAccount.Name;
		(await _repository.DeleteAsync(accountId, TestUser.Id, dbTransaction)).Should().Be(1);
		firstAccount = await _repository.FindByIdAsync(accountId, TestUser.Id, dbTransaction);
		firstAccount.Should().BeNull();

		counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		counterPartyId = await _counterpartyRepository.AddAsync(counterParty, dbTransaction);
		account = account with { Id = Guid.NewGuid(), CounterpartyId = counterPartyId };
		accountId = await _unitOfWork.AddAsync(account, dbTransaction);
		var secondAccount = await _repository.GetByIdAsync(accountId, TestUser.Id, dbTransaction);
		secondAccount.Name.Should().Be(firstAccountName);
		var secondAccountByName = await _repository.FindByNameAsync(secondAccount.Name, TestUser.Id, dbTransaction);
		secondAccountByName.Should().BeEquivalentTo(secondAccount, Options);
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

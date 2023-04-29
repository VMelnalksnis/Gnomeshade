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
		_unitOfWork = new(_dbConnection, _repository, _inCurrencyRepository, new(NullLogger<CounterpartyRepository>.Instance, _dbConnection));
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

		await _unitOfWork.DeleteAsync(getAccount, TestUser.Id);
	}

	[Test]
	public async Task FindByName_ShouldIgnoreDeleted()
	{
		var currencies = await EntityFactory.GetCurrenciesAsync();
		var preferredCurrency = currencies.First();

		var counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		var counterPartyId = await _counterpartyRepository.AddAsync(counterParty);
		counterParty = await _counterpartyRepository.GetByIdAsync(counterPartyId, TestUser.Id);

		var accountFaker = new AccountFaker(TestUser, counterParty, preferredCurrency);
		var account = accountFaker.Generate();
		var accountId = await _unitOfWork.AddAsync(account);

		var firstAccount = await _repository.GetByIdAsync(accountId, TestUser.Id);
		var firstAccountName = firstAccount.Name;
		await _repository.DeleteAsync(accountId, TestUser.Id);
		firstAccount = await _repository.FindByIdAsync(accountId, TestUser.Id);
		firstAccount.Should().BeNull();

		counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		counterPartyId = await _counterpartyRepository.AddAsync(counterParty);
		account = account with { Id = Guid.NewGuid(), CounterpartyId = counterPartyId };
		accountId = await _unitOfWork.AddAsync(account);
		var secondAccount = await _repository.GetByIdAsync(accountId, TestUser.Id);
		secondAccount.Name.Should().Be(firstAccountName);
		var secondAccountByName = await _repository.FindByNameAsync(secondAccount.Name, TestUser.Id);
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

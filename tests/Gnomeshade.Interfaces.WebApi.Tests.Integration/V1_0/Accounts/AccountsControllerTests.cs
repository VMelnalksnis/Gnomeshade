// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

using NodaTime;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Accounts;

[TestOf(typeof(AccountsController))]
public class AccountsControllerTests
{
	private IGnomeshadeClient _client = null!;
	private Currency _firstCurrency = null!;
	private Currency _secondCurrency = null!;
	private Counterparty _counterparty = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
		var currencies = await _client.GetCurrenciesAsync();
		_firstCurrency = currencies.First();
		_secondCurrency = currencies.Skip(1).First();
		_counterparty = await _client.GetMyCounterpartyAsync();
	}

	[Test]
	public async Task AddCurrency()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);

		var accountId = await _client.CreateAccountAsync(accountCreationModel);

		var newAccountInCurrency = new AccountInCurrencyCreationModel { CurrencyId = _secondCurrency.Id };
		var addCurrencyId = await _client.AddCurrencyToAccountAsync(accountId, newAccountInCurrency);

		var account = await _client.GetAccountAsync(accountId);
		var accounts = await _client.GetAccountsAsync();
		accounts.Should().Contain(a => a.Id == account.Id);

		addCurrencyId.Should().Be(accountId);
		account.Currencies
			.Should()
			.SatisfyRespectively(
				inCurrency => inCurrency.Currency.Id.Should().Be(_firstCurrency.Id),
				inCurrency => inCurrency.Currency.Id.Should().Be(_secondCurrency.Id));
	}

	[Test]
	public async Task Put()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);
		var accountId = Guid.NewGuid();

		await _client.PutAccountAsync(accountId, accountCreationModel);

		var createdAccount = await _client.GetAccountAsync(accountId);
		createdAccount.Id.Should().Be(accountId);
		accountCreationModel.Currencies!.Add(new() { CurrencyId = _secondCurrency.Id });

		await _client.PutAccountAsync(accountId, accountCreationModel);

		var updatedAccount = await _client.GetAccountAsync(accountId);
		updatedAccount.Should().BeEquivalentTo(createdAccount, options => options.Excluding(a => a.ModifiedAt));
		updatedAccount.ModifiedAt.Should().BeGreaterThan(createdAccount.ModifiedAt);
	}

	[Test]
	public async Task Put_ShouldReturnConflictOnDuplicateName()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);
		await _client.PutAccountAsync(Guid.NewGuid(), accountCreationModel);

		var exception =
			await FluentActions
				.Awaiting(() => _client.PutAccountAsync(Guid.NewGuid(), accountCreationModel))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>();

		exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	[Test]
	public async Task Put_DuplicateNameDifferentCounterparty()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);
		await _client.PutAccountAsync(Guid.NewGuid(), accountCreationModel);

		var differentCounterparty = await (await WebserverSetup.CreateAuthorizedSecondClientAsync()).GetMyCounterpartyAsync();
		var secondAccount = accountCreationModel with
		{
			CounterpartyId = differentCounterparty.Id,
		};

		await _client.PutAccountAsync(Guid.NewGuid(), secondAccount);
	}

	[Test]
	public async Task Balance_ShouldReturnExpected()
	{
		var firstAccountModel = GetAccountCreationModel(_firstCurrency);
		var firstAccountId = Guid.NewGuid();
		await _client.PutAccountAsync(firstAccountId, firstAccountModel);
		await _client.AddCurrencyToAccountAsync(firstAccountId, new() { CurrencyId = _secondCurrency.Id });
		var firstAccount = await _client.GetAccountAsync(firstAccountId);

		var secondAccountModel = GetAccountCreationModel(_secondCurrency);
		var secondAccountId = Guid.NewGuid();
		await _client.PutAccountAsync(secondAccountId, secondAccountModel);
		var secondAccount = await _client.GetAccountAsync(secondAccountId);

		var transaction = new TransactionCreationModel
		{
			BookedAt = SystemClock.Instance.GetCurrentInstant(),
		};
		var transactionId = Guid.NewGuid();
		await _client.PutTransactionAsync(transactionId, transaction);

		var firstTransfer = new TransferCreation
		{
			SourceAccountId = firstAccount.Currencies.First().Id,
			SourceAmount = 5m,
			TargetAccountId = secondAccount.Currencies.Single().Id,
			TargetAmount = 7.5m,
		};
		await _client.PutTransferAsync(transactionId, Guid.NewGuid(), firstTransfer);

		var secondTransfer = new TransferCreation
		{
			SourceAccountId = secondAccount.Currencies.Single().Id,
			SourceAmount = 15m,
			TargetAccountId = firstAccount.Currencies.First().Id,
			TargetAmount = 10m,
		};
		await _client.PutTransferAsync(transactionId, Guid.NewGuid(), secondTransfer);

		var thirdTransfer = new TransferCreation
		{
			SourceAccountId = secondAccount.Currencies.Single().Id,
			SourceAmount = 3,
			TargetAccountId = firstAccount.Currencies.Last().Id,
			TargetAmount = 3,
		};
		await _client.PutTransferAsync(transactionId, Guid.NewGuid(), thirdTransfer);

		var expectedBalances = new List<Balance>
		{
			new() { AccountInCurrencyId = firstAccount.Currencies.First().Id, SourceAmount = 5, TargetAmount = 10 },
			new() { AccountInCurrencyId = firstAccount.Currencies.Last().Id, SourceAmount = 0, TargetAmount = 3 },
		};

		var firstBalances = await _client.GetAccountBalanceAsync(firstAccountId);
		firstBalances.Should().BeEquivalentTo(expectedBalances);

		var secondBalances = await _client.GetAccountBalanceAsync(secondAccountId);
		var balance = secondBalances.Should().ContainSingle().Subject;
		balance.SourceAmount.Should().Be(18);
		balance.TargetAmount.Should().Be(7.5m);
	}

	private AccountCreationModel GetAccountCreationModel(Currency currency)
	{
		return new()
		{
			Name = Guid.NewGuid().ToString("N"),
			CounterpartyId = _counterparty.Id,
			PreferredCurrencyId = currency.Id,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};
	}
}

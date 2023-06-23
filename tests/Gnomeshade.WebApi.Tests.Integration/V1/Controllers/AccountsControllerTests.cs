// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(AccountsController))]
public sealed class AccountsControllerTests : WebserverTests
{
	private IGnomeshadeClient _client = null!;
	private Currency _firstCurrency = null!;
	private Currency _secondCurrency = null!;
	private Counterparty _counterparty = null!;

	public AccountsControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
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
		var firstInCurrencyId = (await _client.GetAccountAsync(accountId)).Currencies.Single().Id;

		var newAccountInCurrency = new AccountInCurrencyCreation { CurrencyId = _secondCurrency.Id };
		var addCurrencyId = await _client.AddCurrencyToAccountAsync(accountId, newAccountInCurrency);

		var account = await _client.GetAccountAsync(accountId);
		var accounts = await _client.GetAccountsAsync();
		accounts.Should().Contain(a => a.Id == accountId);

		addCurrencyId.Should().Be(account.Currencies.Single(c => c.Id != firstInCurrencyId).Id);
		var currencies = account.Currencies.OrderBy(c => c.CreatedAt).ToArray();
		currencies
			.Should()
			.SatisfyRespectively(
				inCurrency => inCurrency.CurrencyId.Should().Be(_firstCurrency.Id),
				inCurrency => inCurrency.CurrencyId.Should().Be(_secondCurrency.Id));

		await _client.RemoveCurrencyFromAccountAsync(accountId, currencies.First().Id);
		account = await _client.GetAccountAsync(accountId);
		account.Currencies.Should().ContainSingle().Which.CurrencyId.Should().Be(_secondCurrency.Id);
	}

	[Test]
	public async Task RemoveCurrency()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);

		var accountId = await _client.CreateAccountAsync(accountCreationModel);

		var newAccountInCurrency = new AccountInCurrencyCreation { CurrencyId = _secondCurrency.Id };
		var addCurrencyId = await _client.AddCurrencyToAccountAsync(accountId, newAccountInCurrency);
		var account = await _client.GetAccountAsync(accountId);

		var transactionId = await _client.CreateTransactionAsync(new());
		var transferId = Guid.NewGuid();
		var transfer = new TransferCreation
		{
			TransactionId = transactionId,
			SourceAccountId = account.Currencies.First(currency => currency.Id != addCurrencyId).Id,
			TargetAccountId = addCurrencyId,
			SourceAmount = 10.5m,
			TargetAmount = 10.5m,
			BookedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		await _client.PutTransferAsync(transferId, transfer);

		await ShouldThrowConflict(() => _client.RemoveCurrencyFromAccountAsync(accountId, addCurrencyId));

		var accountAfterDelete = await _client.GetAccountAsync(accountId);
		accountAfterDelete.Should().BeEquivalentTo(account);

		await _client.DeleteTransferAsync(transferId);
		await _client.RemoveCurrencyFromAccountAsync(accountId, addCurrencyId);

		accountAfterDelete = await _client.GetAccountAsync(accountId);
		accountAfterDelete.Currencies.Should().ContainSingle();
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
		updatedAccount.ModifiedAt.Should().BeGreaterThanOrEqualTo(createdAccount.ModifiedAt);
	}

	[Test]
	public async Task Put_ShouldReturnConflictOnDuplicateName()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);
		await _client.PutAccountAsync(Guid.NewGuid(), accountCreationModel);

		await ShouldThrowConflict(() => _client.PutAccountAsync(Guid.NewGuid(), accountCreationModel));
	}

	[Test]
	public async Task Put_DuplicateNameDifferentCounterparty()
	{
		var accountCreationModel = GetAccountCreationModel(_firstCurrency);
		await _client.PutAccountAsync(Guid.NewGuid(), accountCreationModel);

		var differentCounterparty = await (await Fixture.CreateAuthorizedClientAsync()).GetMyCounterpartyAsync();
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

		await PutTransactionAndTransfer(firstAccount.Currencies.First(), 5m, secondAccount.Currencies.Single(), 7.5m);
		await PutTransactionAndTransfer(firstAccount.Currencies.First(), 5m, secondAccount.Currencies.Single(), 7.5m);
		await PutTransactionAndTransfer(secondAccount.Currencies.Single(), 15m, firstAccount.Currencies.First(), 10m);
		await PutTransactionAndTransfer(secondAccount.Currencies.Single(), 3m, firstAccount.Currencies.Last(), 3m);

		var transactionId = await PutTransactionAndTransfer(secondAccount.Currencies.Single(), 3m, firstAccount.Currencies.Last(), 3m);
		var transfers = await _client.GetTransfersAsync(transactionId);
		foreach (var transfer in transfers)
		{
			await _client.DeleteTransferAsync(transfer.Id);
		}

		await _client.DeleteTransactionAsync(transactionId);

		var expectedBalances = new List<Balance>
		{
			new() { AccountInCurrencyId = firstAccount.Currencies.First().Id, SourceAmount = 10, TargetAmount = 10 },
			new() { AccountInCurrencyId = firstAccount.Currencies.Last().Id, SourceAmount = 0, TargetAmount = 3 },
		};

		var firstBalances = await _client.GetAccountBalanceAsync(firstAccountId);
		firstBalances.Should().BeEquivalentTo(expectedBalances);

		var secondBalances = await _client.GetAccountBalanceAsync(secondAccountId);
		var balance = secondBalances.Should().ContainSingle().Subject;
		balance.SourceAmount.Should().Be(18);
		balance.TargetAmount.Should().Be(15m);
	}

	private async Task<Guid> PutTransactionAndTransfer(
		AccountInCurrency source,
		decimal sourceAmount,
		AccountInCurrency target,
		decimal targetAmount)
	{
		var transactionId = Guid.NewGuid();
		await _client.PutTransactionAsync(transactionId, new());

		var transfer = new TransferCreation
		{
			TransactionId = transactionId,
			SourceAccountId = source.Id,
			SourceAmount = sourceAmount,
			TargetAccountId = target.Id,
			TargetAmount = targetAmount,
			BookedAt = SystemClock.Instance.GetCurrentInstant(),
		};
		await _client.PutTransferAsync(Guid.NewGuid(), transfer);
		return transactionId;
	}

	private AccountCreation GetAccountCreationModel(Currency currency)
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

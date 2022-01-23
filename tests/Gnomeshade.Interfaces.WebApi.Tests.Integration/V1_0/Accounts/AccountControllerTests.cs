// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Accounts;

public class AccountControllerTests
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
		updatedAccount.ModifiedAt.Should().BeAfter(createdAccount.ModifiedAt);
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

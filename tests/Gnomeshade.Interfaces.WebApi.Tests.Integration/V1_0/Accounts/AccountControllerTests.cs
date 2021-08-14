// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Accounts
{
	public class AccountControllerTests
	{
		private IGnomeshadeClient _client = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_client = await WebserverSetup.CreateAuthorizedClientAsync();
		}

		[Test]
		public async Task AddCurrency()
		{
			var currencies = await _client.GetCurrenciesAsync();
			var existingCurrency = currencies.First();
			var newCurrency = currencies.Skip(1).First();

			var counterparty = await _client.GetMyCounterpartyAsync();
			var accountCreationModel = new AccountCreationModel
			{
				Name = Guid.NewGuid().ToString("N"),
				CounterpartyId = counterparty.Id,
				PreferredCurrencyId = existingCurrency.Id,
				Currencies = new() { new() { CurrencyId = existingCurrency.Id } },
			};

			var accountId = await _client.CreateAccountAsync(accountCreationModel);

			var newAccountInCurrency = new AccountInCurrencyCreationModel { CurrencyId = newCurrency.Id };
			var addCurrencyId = await _client.AddCurrencyToAccountAsync(accountId, newAccountInCurrency);

			var account = await _client.GetAccountAsync(accountId);
			var accounts = await _client.GetAccountsAsync();
			accounts.Should().Contain(a => a.Id == account.Id);

			addCurrencyId.Should().Be(accountId);
			account.Currencies
				.Should()
				.SatisfyRespectively(
					inCurrency => inCurrency.Currency.Id.Should().Be(existingCurrency.Id),
					inCurrency => inCurrency.Currency.Id.Should().Be(newCurrency.Id));
		}
	}
}

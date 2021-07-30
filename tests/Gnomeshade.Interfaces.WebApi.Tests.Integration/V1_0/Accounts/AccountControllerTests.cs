// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Accounts
{
	public class AccountControllerTests : IDisposable
	{
		private HttpClient _client = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_client = await WebserverSetup.CreateAuthorizedClientAsync();
		}

		[Test]
		public async Task AddCurrency()
		{
			var currencies = (await _client.GetFromJsonAsync<List<CurrencyModel>>("/api/v1.0/currency"))!;
			var existingCurrency = currencies.First();
			var newCurrency = currencies.Skip(1).First();

			var accountCreationModel = new AccountCreationModel
			{
				Name = Guid.NewGuid().ToString("N"),
				PreferredCurrencyId = existingCurrency.Id,
				Currencies = new() { new() { CurrencyId = existingCurrency.Id } },
			};

			var accountCreationResponse = await _client.PostAsJsonAsync("/api/v1.0/account", accountCreationModel);
			accountCreationResponse.EnsureSuccessStatusCode();
			var accountId = await accountCreationResponse.Content.ReadFromJsonAsync<Guid>();
			var accountUri = $"/api/v1.0/Account/{accountId:N}";

			var newAccountInCurrency = new AccountInCurrencyCreationModel { CurrencyId = newCurrency.Id };
			var addCurrencyResponse = await _client.PostAsJsonAsync(accountUri, newAccountInCurrency);
			addCurrencyResponse.EnsureSuccessStatusCode();
			var addCurrencyId = await addCurrencyResponse.Content.ReadFromJsonAsync<Guid>();

			var account = (await _client.GetFromJsonAsync<AccountModel>(accountUri))!;
			var accounts = (await _client.GetFromJsonAsync<List<AccountModel>>("/api/v1.0/Account"))!;
			accounts.Should().Contain(a => a.Id == account.Id);

			addCurrencyId.Should().Be(accountId);
			account.Currencies
				.Should()
				.SatisfyRespectively(
					inCurrency => inCurrency.Currency.Id.Should().Be(existingCurrency.Id),
					inCurrency => inCurrency.Currency.Id.Should().Be(newCurrency.Id));
		}

		[TearDown]
		public void Dispose()
		{
			_client.Dispose();
		}
	}
}

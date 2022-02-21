// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Accounts;

public class CounterpartyControllerTests
{
	private IGnomeshadeClient _gnomeshadeClient = null!;
	private Currency _currency = null!;

	[SetUp]
	public async Task SetUp()
	{
		_gnomeshadeClient = await WebserverSetup.CreateAuthorizedClientAsync();
		_currency = (await _gnomeshadeClient.GetCurrenciesAsync()).First();
	}

	[Test]
	public async Task Merge_MoveAccountsToTargetCounterparty()
	{
		var (firstCounterpartyId, firstAccountId) = await CreateCounterpartyWithAccount();
		var (secondCounterpartyId, secondAccountId) = await CreateCounterpartyWithAccount();

		await _gnomeshadeClient.MergeCounterpartiesAsync(firstCounterpartyId, secondCounterpartyId);
		var firstAccount = await _gnomeshadeClient.GetAccountAsync(firstAccountId);
		var secondAccount = await _gnomeshadeClient.GetAccountAsync(secondAccountId);

		using (new AssertionScope())
		{
			firstAccount.CounterpartyId.Should().Be(firstCounterpartyId);
			secondAccount.CounterpartyId.Should().Be(firstCounterpartyId);

			await FluentActions
				.Awaiting(() => _gnomeshadeClient.GetCounterpartyAsync(firstCounterpartyId))
				.Should()
				.NotThrowAsync();

			(await FluentActions
					.Awaiting(() => _gnomeshadeClient.GetCounterpartyAsync(secondCounterpartyId))
					.Should()
					.ThrowAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(HttpStatusCode.NotFound);
		}
	}

	private async Task<(Guid CounterpartyId, Guid AccountId)> CreateCounterpartyWithAccount()
	{
		var counterpartyId = await _gnomeshadeClient.CreateCounterpartyAsync(new()
		{
			Name = $"{Guid.NewGuid():N}",
		});

		var accountId = await _gnomeshadeClient.CreateAccountAsync(new()
		{
			Name = $"{Guid.NewGuid()}",
			CounterpartyId = counterpartyId,
			Currencies = new() { new() { CurrencyId = _currency.Id } },
			PreferredCurrencyId = _currency.Id,
		});

		return (counterpartyId, accountId);
	}
}

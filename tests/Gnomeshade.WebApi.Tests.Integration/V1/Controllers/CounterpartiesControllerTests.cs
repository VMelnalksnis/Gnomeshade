// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(CounterpartiesController))]
public class CounterpartiesControllerTests : WebserverTests
{
	private IGnomeshadeClient _gnomeshadeClient = null!;
	private Currency _currency = null!;

	public CounterpartiesControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUp()
	{
		_gnomeshadeClient = await Fixture.CreateAuthorizedClientAsync();
		_currency = (await _gnomeshadeClient.GetCurrenciesAsync()).First();
	}

	[Test]
	public async Task Put_ShouldUpdateExisting()
	{
		var id = Guid.NewGuid();
		var creationModel = new CounterpartyCreation { Name = $"{id:N}" };

		await ShouldThrowNotFound(() => _gnomeshadeClient.GetCounterpartyAsync(id));

		await _gnomeshadeClient.PutCounterpartyAsync(id, creationModel);

		var counterparty = await _gnomeshadeClient.GetCounterpartyAsync(id);

		creationModel = new() { Name = $"{Guid.NewGuid():N}" };
		await _gnomeshadeClient.PutCounterpartyAsync(id, creationModel);

		var updatedCounterparty = await _gnomeshadeClient.GetCounterpartyAsync(id);
		updatedCounterparty.ModifiedAt.Should().BeGreaterThanOrEqualTo(counterparty.ModifiedAt);
	}

	[Test]
	public async Task Put_ShouldReturnConflictOnDuplicateName()
	{
		var existingName = (await _gnomeshadeClient.GetCounterpartiesAsync()).First().Name;

		await ShouldThrowConflict(() => _gnomeshadeClient.PutCounterpartyAsync(Guid.NewGuid(), new() { Name = existingName }));
	}

	[Test]
	public async Task Merge_MoveAccountsToTargetCounterparty()
	{
		var (firstCounterpartyId, firstAccountId) = await CreateCounterpartyWithAccount();
		var (secondCounterpartyId, secondAccountId) = await CreateCounterpartyWithAccount();

		await _gnomeshadeClient.MergeCounterpartiesAsync(firstCounterpartyId, secondCounterpartyId);
		var firstAccount = await _gnomeshadeClient.GetAccountAsync(firstAccountId);
		var secondAccount = await _gnomeshadeClient.GetAccountAsync(secondAccountId);

		using var scope = new AssertionScope();

		firstAccount.CounterpartyId.Should().Be(firstCounterpartyId);
		secondAccount.CounterpartyId.Should().Be(firstCounterpartyId);

		await ShouldThrowNotFound(() => _gnomeshadeClient.GetCounterpartyAsync(secondCounterpartyId));
		_ = await _gnomeshadeClient.GetCounterpartyAsync(firstCounterpartyId);
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

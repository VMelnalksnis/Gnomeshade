// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V2.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V2.Controllers;

[TestOf(typeof(LoansController))]
public sealed class LoansControllerTests(WebserverFixture fixture) : WebserverTests(fixture)
{
	private IGnomeshadeClient _client = null!;

	[SetUp]
	public async Task SetUp()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Create_Read_Update_Delete()
	{
		var first = await _client.CreateCounterpartyAsync();
		var second = await _client.CreateCounterpartyAsync();
		var currency = await _client.GetCurrencyAsync();

		var creation = new LoanCreation
		{
			Name = "Mortgage",
			IssuingCounterpartyId = first.Id,
			ReceivingCounterpartyId = second.Id,
			Principal = 10_000m,
			CurrencyId = currency.Id,
		};

		var id = await _client.CreateLoanAsync(creation);
		var loan = await _client.GetLoanAsync(id);
		var loans = await _client.GetLoansAsync();

		using (new AssertionScope())
		{
			loan.Name.Should().Be("Mortgage");
			loan.IssuingCounterpartyId.Should().Be(first.Id);
			loan.ReceivingCounterpartyId.Should().Be(second.Id);
			loan.Principal.Should().Be(10_000m);
			loan.CurrencyId.Should().Be(currency.Id);

			loans.Should().ContainSingle().Which.Should().BeEquivalentTo(loan);
		}

		var changedName = $"{creation.Name} {id}";
		await _client.PutLoanAsync(id, creation with { Name = changedName });
		var updatedLoan = await _client.GetLoanAsync(id);

		using (new AssertionScope())
		{
			updatedLoan.Name.Should().Be(changedName);
			updatedLoan.ModifiedAt.Should().BeGreaterThan(updatedLoan.CreatedAt);
			updatedLoan.CreatedAt.Should().Be(loan.CreatedAt);
		}

		await _client.DeleteLoanAsync(id);

		loans = await _client.GetLoansAsync();
		loans.Should().NotContain(l => l.Id == id);
	}
}

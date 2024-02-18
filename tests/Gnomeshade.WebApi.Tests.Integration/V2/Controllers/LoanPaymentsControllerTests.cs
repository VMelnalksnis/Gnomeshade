// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V2.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V2.Controllers;

[TestOf(typeof(LoanPaymentsController))]
public sealed class LoanPaymentsControllerTests(WebserverFixture fixture) : WebserverTests(fixture)
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
		var loan = await _client.CreateLoanAsync();
		var transaction = await _client.CreateTransactionAsync();

		var creation = new LoanPaymentCreation
		{
			LoanId = loan.Id,
			TransactionId = transaction.Id,
			Amount = 500m,
			Interest = 150m,
		};

		var id = await _client.CreateLoanPaymentAsync(creation);
		var payment = await _client.GetLoanPaymentAsync(id);
		var allPayments = await _client.GetLoanPaymentsAsync();
		var loanPayments = await _client.GetLoanPaymentsAsync(loan.Id);
		var transactionPayments = await _client.GetLoanPaymentsForTransactionAsync(transaction.Id);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transaction.Id);

		using (new AssertionScope())
		{
			payment.LoanId.Should().Be(loan.Id);
			payment.TransactionId.Should().Be(transaction.Id);
			payment.Amount.Should().Be(500m);
			payment.Interest.Should().Be(150m);

			allPayments.Should().ContainEquivalentOf(payment);
			loanPayments.Should().ContainSingle().Which.Should().BeEquivalentTo(payment);
			transactionPayments.Should().ContainSingle().Which.Should().BeEquivalentTo(payment);
			detailedTransaction.LoanPayments.Should().ContainSingle().Which.Should().BeEquivalentTo(payment);
		}

		var newInterest = creation.Interest + 10m;
		await _client.PutLoanPaymentAsync(id, creation with { Interest = newInterest });
		var updatedPayment = await _client.GetLoanPaymentAsync(id);

		using (new AssertionScope())
		{
			updatedPayment.Interest.Should().Be(newInterest);
			updatedPayment.ModifiedAt.Should().BeGreaterThan(updatedPayment.CreatedAt);
			updatedPayment.CreatedAt.Should().Be(payment.CreatedAt);
		}

		await _client.DeleteLoanPaymentAsync(id);

		loanPayments = await _client.GetLoanPaymentsAsync(loan.Id);
		transactionPayments = await _client.GetLoanPaymentsForTransactionAsync(transaction.Id);
		detailedTransaction = await _client.GetDetailedTransactionAsync(transaction.Id);

		using (new AssertionScope())
		{
			(await FluentActions
					.Awaiting(() => _client.GetLoanPaymentAsync(id))
					.Should()
					.ThrowExactlyAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(HttpStatusCode.NotFound);

			loanPayments.Should().NotContain(loanPayment => loanPayment.Id == id);
			loanPayments.Should().BeEmpty();
			transactionPayments.Should().BeEmpty();
			detailedTransaction.LoanPayments.Should().BeEmpty();
		}
	}
}

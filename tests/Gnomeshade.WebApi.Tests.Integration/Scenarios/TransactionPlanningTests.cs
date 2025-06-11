using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.Scenarios;

public sealed class TransactionPlanningTests(WebserverFixture fixture) : WebserverTests(fixture)
{
	[Test]
	public async Task Test()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var bankCounterparty = await client.CreateCounterpartyAsync();
		var bankAccount = await client.CreateAccountAsync(bankCounterparty.Id);

		var counterparty = await client.GetMyCounterpartyAsync();
		var account = await client.CreateAccountAsync(counterparty.Id);

		var productId = await client.CreateProductAsync(new() { Name = Guid.NewGuid().ToString("N") });
		var loan = await client.CreateLoanAsync(bankCounterparty.Id, counterparty.Id);

		var scheduleId = await client.CreateTransactionSchedule(new()
		{
			Name = Guid.NewGuid().ToString("N"),
			StartingAt = Instant.FromUtc(2024, 10, 15, 08, 00, 00),
			Period = Period.FromMonths(1),
			Count = 120,
		});

		var transactionId = await client.CreatePlannedTransaction(new() { ScheduleId = scheduleId });

		var principalTransferId = await client.CreatePlannedTransfer(new()
		{
			SourceAmount = 500,
			TargetAmount = 500,
			TransactionId = transactionId,
			SourceAccountId = account.Currencies.Single().Id,
			TargetCounterpartyId = bankCounterparty.Id,
			TargetCurrencyId = bankAccount.Currencies.Single().CurrencyId,
			BookedAt = Instant.FromUtc(2024, 10, 15, 08, 00, 00),
		});

		var interestTransferId = await client.CreatePlannedTransfer(new()
		{
			SourceAmount = 100,
			TargetAmount = 100,
			TransactionId = transactionId,
			SourceAccountId = account.Currencies.Single().Id,
			TargetCounterpartyId = bankCounterparty.Id,
			TargetCurrencyId = bankAccount.Currencies.Single().CurrencyId,
			BookedAt = Instant.FromUtc(2024, 10, 15, 08, 00, 00),
		});

		var purchaseId = await client.CreatePlannedPurchase(new()
		{
			TransactionId = transactionId,
			Price = 600,
			CurrencyId = bankAccount.Currencies.Single().CurrencyId,
			ProductId = productId,
			Amount = 1,
		});

		var loanPaymentId = await client.CreatePlannedLoanPayment(new()
		{
			LoanId = loan.Id,
			TransactionId = transactionId,
			Amount = 500,
			Interest = 100,
		});
	}
}

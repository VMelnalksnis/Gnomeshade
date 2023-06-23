// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.Tests.Integration.V1.Controllers.Iso20022;
using Gnomeshade.WebApi.V1.Controllers;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(Iso20022Controller))]
public sealed class Iso20022ControllerTests : WebserverTests
{
	private const string _fileName = "report.xml";

	private IGnomeshadeClient _client = null!;

	public Iso20022ControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[OneTimeSetUp]
	public async Task OneTimeSetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Import_ShouldNotCreateDuplicateTransactions()
	{
		var testCase = TestData.Sample;
		await using var contentStream = testCase.Stream;

		var reportResult = await _client.Import(contentStream, _fileName);

		using (new AssertionScope())
		{
			reportResult.Should().NotBeNull();
			reportResult.TransferReferences.Should().HaveCount(2);

			reportResult.AccountReferences
				.Should()
				.HaveCount(3)
				.And.ContainSingle(reference => reference.Account.Iban == testCase.AccountIban);

			reportResult.TransactionReferences
				.Should()
				.HaveCount(2)
				.And.AllSatisfy(reference =>
				{
					reference.Created.Should().BeTrue();
					reference.Transaction.ImportedAt.Should().NotBeNull();
				});
		}

		contentStream.Seek(0, SeekOrigin.Begin);
		var secondReportResult = await _client.Import(contentStream, _fileName);

		secondReportResult.TransactionReferences
			.Should()
			.AllSatisfy(reference => reference.Created.Should().BeFalse());
	}

	[Test]
	public async Task Import_ShouldUpdateExistingTransfer()
	{
		var firstTestCase = TestData.Account1;
		var secondTestCase = TestData.Account2;

		var counterparty = await _client.GetMyCounterpartyAsync();
		var currencies = await _client.GetCurrenciesAsync();
		var euro = currencies.Should().ContainSingle(currency => currency.AlphabeticCode == "EUR").Subject!;
		var dollar = currencies.Should().ContainSingle(currency => currency.AlphabeticCode == "USD").Subject!;

		var accounts = await _client.GetAccountsAsync();
		const string bankAccountName = "TEST BANK";
		var bankAccount = accounts.SingleOrDefault(account => account.Name == bankAccountName);
		if (bankAccount is null)
		{
			var counterpartyId = await _client.CreateCounterpartyAsync(new() { Name = bankAccountName });
			var bankAccountCreation = new AccountCreation
			{
				Name = bankAccountName,
				CounterpartyId = counterpartyId,
				PreferredCurrencyId = euro.Id,
				Bic = "TESTLV01",
				Currencies = new() { new() { CurrencyId = euro.Id } },
			};
			var bankAccountId = await _client.CreateAccountAsync(bankAccountCreation);
			bankAccount = await _client.GetAccountAsync(bankAccountId);
		}

		var accountCreation = new AccountCreation
		{
			Name = firstTestCase.AccountIban,
			CounterpartyId = counterparty.Id,
			PreferredCurrencyId = euro.Id,
			Iban = firstTestCase.AccountIban,
			AccountNumber = firstTestCase.AccountIban,
			Currencies = new() { new() { CurrencyId = euro.Id }, new() { CurrencyId = dollar.Id } },
		};

		var accountId = await _client.CreateAccountAsync(accountCreation);
		var account = await _client.GetAccountAsync(accountId);

		var transactionCreation = new TransactionCreation
		{
			Description = "REIMBURSEMENT OF COMMISSION",
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreation);

		var transferCreation = new TransferCreation
		{
			TransactionId = transactionId,
			SourceAmount = 235,
			SourceAccountId = account.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
			TargetAmount = 235,
			TargetAccountId = bankAccount.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
			ExternalReference = "ABC123456",
			Order = 0,
			BookedAt = Instant.FromUtc(2023, 03, 14, 0, 0, 0),
		};

		var transferId = Guid.NewGuid();
		await _client.PutTransferAsync(transferId, transferCreation);

		await _client.RemoveCurrencyFromAccountAsync(accountId, account.Currencies.Single(c => c.CurrencyId == dollar.Id).Id);

		var firstAccountResult = await _client.Import(firstTestCase.Stream, _fileName);
		var secondAccountResult = await _client.Import(secondTestCase.Stream, _fileName);

		using (new AssertionScope())
		{
			var existingTransfer = firstAccountResult.TransferReferences
				.Should()
				.HaveCount(4).And.ContainSingle(reference => reference.Created == false)
				.Subject.Transfer;

			existingTransfer.Id.Should().Be(transferId);
			existingTransfer.BankReference.Should().Be("123456789876543.020001");

			secondAccountResult.TransferReferences.Should().HaveCount(2);
			secondAccountResult.TransferReferences.Should().ContainSingle(transfer => transfer.Created == true);
			secondAccountResult.TransferReferences
				.Should()
				.ContainSingle(transfer => transfer.Created == false)
				.Which.Transfer.ExternalReference
				.Should()
				.Be("AB12345ABCDE");
		}
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.V1.Importing;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;
using VMelnalksnis.NordigenDotNet.Accounts;

namespace Gnomeshade.WebApi.Tests.V1.Importing;

public sealed class BookedTransactionExtensionsTests
{
	[TestCase("CASH WITHDRAWAL", "PMNT-CCRD-CWDL", CreditDebitCode.DBIT)]
	public void GetCreditDebitCode(string? information, string? code, CreditDebitCode creditDebitCode)
	{
		new BookedTransaction { AdditionalInformation = information, BankTransactionCode = code }
			.GetCreditDebitCode()
			.Should()
			.Be(creditDebitCode);
	}

	[TestCase(null, null, null, null)]
	[TestCase("PMNT-CCRD-CWDL", "PMNT", "CCRD", "CWDL")]
	public void GetCode(string? code, string? domain, string? family, string? subFamily)
	{
		new BookedTransaction { BankTransactionCode = code }
			.GetCode()
			.Should()
			.Be((domain, family, subFamily));
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes;

internal static class BankTransactionCodeStructureExtensions
{
	internal static StandardCode? GetStandardCode(this BankTransactionCodeStructure4 bankTransactionCodeStructure)
	{
		if (bankTransactionCodeStructure.Domain is null)
		{
			return null;
		}

		var domain = Domain.FromName(bankTransactionCodeStructure.Domain.Code);
		var family = Family.FromName(bankTransactionCodeStructure.Domain.Family.Code);
		var subfamily = SubFamily.FromName(bankTransactionCodeStructure.Domain.Family.SubFamilyCode);
		return new(domain, family, subfamily);
	}

	internal sealed record StandardCode(Domain Domain, Family Family, SubFamily SubFamily);
}

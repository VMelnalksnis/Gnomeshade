// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

namespace Gnomeshade.WebApi.V1.Importing.TransactionCodes;

internal static class BankTransactionCodeStructureExtensions
{
	internal static SubFamily? GetStandardCode(this BankTransactionCodeStructure4 bankTransactionCodeStructure)
	{
		if (bankTransactionCodeStructure.Domain is null)
		{
			return null;
		}

		var subfamily = SubFamily.FromName(bankTransactionCodeStructure.Domain.Family.SubFamilyCode);
		return subfamily;
	}
}

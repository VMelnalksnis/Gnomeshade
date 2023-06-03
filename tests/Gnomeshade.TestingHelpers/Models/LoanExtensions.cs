// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.TestingHelpers.Models;

public static class LoanExtensions
{
	public static LoanCreation ToCreation(this Loan loan) => new()
	{
		OwnerId = loan.OwnerId,
		TransactionId = loan.TransactionId,
		IssuingCounterpartyId = loan.IssuingCounterpartyId,
		ReceivingCounterpartyId = loan.ReceivingCounterpartyId,
		CurrencyId = loan.CurrencyId,
		Amount = loan.Amount,
	};
}

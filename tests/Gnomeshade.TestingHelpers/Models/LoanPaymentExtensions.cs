// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Loans;

namespace Gnomeshade.TestingHelpers.Models;

public static class LoanPaymentExtensions
{
	public static LoanPaymentCreation ToCreation(this LoanPayment payment) => new()
	{
		OwnerId = payment.OwnerId,
		LoanId = payment.LoanId,
		TransactionId = payment.TransactionId,
		Amount = payment.Amount,
		Interest = payment.Interest,
	};
}

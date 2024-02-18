// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Loans;

namespace Gnomeshade.TestingHelpers.Models;

public static class LoanExtensions
{
	public static LoanCreation ToCreation(this Loan loan) => new()
	{
		OwnerId = loan.OwnerId,
		Name = loan.Name,
		IssuingCounterpartyId = loan.IssuingCounterpartyId,
		ReceivingCounterpartyId = loan.ReceivingCounterpartyId,
		Principal = loan.Principal,
		CurrencyId = loan.CurrencyId,
	};
}

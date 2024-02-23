// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans.Migration;

/// <inheritdoc />
internal sealed class LoanCounterpartyComparer : IEqualityComparer<Loan?>
{
	public bool Equals(Loan? x, Loan? y)
	{
		if (ReferenceEquals(x, y))
		{
			return true;
		}

		if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
		{
			return false;
		}

		return
			(x.IssuingCounterpartyId == y.IssuingCounterpartyId && x.ReceivingCounterpartyId == y.ReceivingCounterpartyId) ||
			(x.IssuingCounterpartyId == y.ReceivingCounterpartyId && x.ReceivingCounterpartyId == y.IssuingCounterpartyId);
	}

	public int GetHashCode(Loan obj)
	{
		return HashCode.Combine(obj.IssuingCounterpartyId, obj.ReceivingCounterpartyId);
	}
}

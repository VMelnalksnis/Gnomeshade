// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Reports;

internal static class TransferExtensions
{
	internal static decimal SumForAccounts(this IEnumerable<Transfer> transfers, Guid[] inCurrencyIds)
	{
		decimal sum = 0;

		foreach (var transfer in transfers)
		{
			var sourceSelected = inCurrencyIds.Contains(transfer.SourceAccountId);
			var targetTargetSelected = inCurrencyIds.Contains(transfer.TargetAccountId);

			sum += (sourceSelected, targetTargetSelected) switch
			{
				(true, true) => 0,
				(true, false) => -transfer.SourceAmount,
				(false, true) => transfer.TargetAmount,
				_ => 0,
			};
		}

		return sum;
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

internal static class TransferExtensions
{
	internal static TransferOverview ToOverview(this Transfer transfer, List<Account> accounts)
	{
		var sourceAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
		var sourceCurrency = sourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId).Currency;
		var targetAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
		var targetCurrency = targetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId).Currency;

		return new(
			transfer.Id,
			transfer.SourceAmount,
			sourceAccount.Name,
			sourceCurrency.AlphabeticCode,
			transfer.TargetAmount,
			targetAccount.Name,
			targetCurrency.AlphabeticCode,
			transfer.BankReference,
			transfer.ExternalReference,
			transfer.InternalReference);
	}
}

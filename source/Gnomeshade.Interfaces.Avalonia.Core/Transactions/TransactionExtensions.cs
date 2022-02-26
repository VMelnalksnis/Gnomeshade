// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

internal static class TransactionExtensions
{
	[LinqTunnel]
	[Pure]
	internal static IEnumerable<TransactionOverview> Translate(
		this IEnumerable<Transaction> transactions,
		IReadOnlyCollection<Account> accounts)
	{
		return transactions
			.Select(transaction =>
			{
				var firstItem = transaction.Items.First();
				var sourceAccount = accounts.Single(account =>
					account.Currencies.Any(currency => currency.Id == firstItem.SourceAccountId));
				var targetAccount = accounts.Single(account =>
					account.Currencies.Any(currency => currency.Id == firstItem.TargetAccountId));

				var sourceCurrency = sourceAccount.Currencies.Single(currency => currency.Id == firstItem.SourceAccountId).Currency;
				var targetCurrency = targetAccount.Currencies.Single(currency => currency.Id == firstItem.TargetAccountId).Currency;

				// todo select per currency
				var sourceAmount = transaction.Items.Sum(item => item.SourceAmount);
				var targetAmount = transaction.Items.Sum(item => item.TargetAmount);

				return new TransactionOverview(transaction, sourceAccount, sourceCurrency, sourceAmount, targetAccount, targetCurrency, targetAmount);
			});
	}
}

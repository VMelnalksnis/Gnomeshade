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
		IReadOnlyCollection<Account> accounts,
		IReadOnlyCollection<Counterparty> counterparties,
		Counterparty userCounterparty)
	{
		return transactions
			.Select(transaction =>
			{
				var firstItem = transaction.Items.First();
				var sourceAccount = accounts.Single(account =>
					account.Currencies.Any(currency => currency.Id == firstItem.SourceAccountId));
				var targetAccount = accounts.Single(account =>
					account.Currencies.Any(currency => currency.Id == firstItem.TargetAccountId));
				var sourceCounterparty = sourceAccount.CounterpartyId == userCounterparty.Id
					? null
					: counterparties.Single(counterparty => counterparty.Id == sourceAccount.CounterpartyId);
				var targetCounterparty = targetAccount.CounterpartyId == userCounterparty.Id
					? null
					: counterparties.Single(counterparty => counterparty.Id == targetAccount.CounterpartyId);

				var sourceCurrency = sourceAccount.Currencies
					.Single(currency => currency.Id == firstItem.SourceAccountId).Currency;
				var targetCurrency = targetAccount.Currencies
					.Single(currency => currency.Id == firstItem.TargetAccountId).Currency;

				// todo select per currency
				var sourceAmount = transaction.Items.Sum(item => item.SourceAmount);
				var targetAmount = transaction.Items.Sum(item => item.TargetAmount);

				var sourceAccounts = accounts
					.Where(account =>
						account.Currencies.Any(currency =>
							transaction.Items.Any(item => item.SourceAccountId == currency.Id)))
					.DistinctBy(account => account.Id)
					.ToList();

				var sourceCounterparties = counterparties
					.Where(counterparty => sourceAccounts.Any(account => account.CounterpartyId == counterparty.Id))
					.DistinctBy(counterparty => counterparty.Id)
					.ToList();

				var targetAccounts = accounts
					.Where(account =>
						account.Currencies.Any(currency =>
							transaction.Items.Any(item => item.TargetAccountId == currency.Id)))
					.DistinctBy(account => account.Id)
					.ToList();

				var targetCounterparties = counterparties
					.Where(counterparty => targetAccounts.Any(account => account.CounterpartyId == counterparty.Id))
					.DistinctBy(counterparty => counterparty.Id)
					.ToList();

				if (sourceCounterparties.Count == 1 && targetCounterparties.Count == 1)
				{
					var sourceCounterparty2 = sourceCounterparties.Single();
					var targetCounterparty2 = targetCounterparties.Single();

					if (sourceCounterparty2.Id != targetCounterparty2.Id)
					{
						if (sourceCounterparty2.Id == userCounterparty.Id)
						{
							sourceAmount = -transaction.Items.Sum(item => item.SourceAmount);
						}

						if (targetCounterparty2.Id != userCounterparty.Id)
						{
							targetAmount = -transaction.Items.Sum(item => item.TargetAmount);
						}
					}
				}

				return new TransactionOverview(
					transaction,
					sourceAccount,
					sourceCounterparty,
					sourceCurrency,
					sourceAmount,
					targetAccount,
					targetCounterparty,
					targetCurrency,
					targetAmount);
			});
	}
}

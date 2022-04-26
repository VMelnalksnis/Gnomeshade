// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Avalonia.Core.Accounts;

internal static class AccountExtensions
{
	[LinqTunnel]
	[Pure]
	public static IEnumerable<AccountOverviewRow> Translate(
		this IEnumerable<Account> accounts,
		List<Counterparty> counterparties,
		IEnumerable<Balance> balances)
	{
		return accounts
			.SelectMany(account => account.Currencies.Select(inCurrency => (account, inCurrency)))
			.Select(tuple =>
			{
				var counterparty = counterparties.Single(c => c.Id == tuple.account.CounterpartyId);
				var balance = balances.SingleOrDefault(balance => balance.AccountInCurrencyId == tuple.inCurrency.Id);
				var sum = balance is null ? 0 : balance.TargetAmount - balance.SourceAmount;
				return new AccountOverviewRow(
					tuple.account.Id,
					tuple.account.Name,
					tuple.inCurrency.Currency.AlphabeticCode,
					sum,
					tuple.inCurrency.Disabled,
					counterparty.Name);
			});
	}
}

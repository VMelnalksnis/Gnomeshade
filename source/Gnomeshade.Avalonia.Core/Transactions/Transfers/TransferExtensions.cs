// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

internal static class TransferExtensions
{
	internal static TransferOverview ToOverview(this Transfer transfer, List<Account> accounts, DateTimeZone timeZone)
	{
		var sourceAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
		var sourceCurrency = sourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId);
		var targetAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
		var targetCurrency = targetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId);

		return new(
			transfer.Id,
			transfer.SourceAmount,
			sourceAccount.Name,
			sourceCurrency.CurrencyAlphabeticCode,
			transfer.TargetAmount,
			targetAccount.Name,
			targetCurrency.CurrencyAlphabeticCode,
			transfer.Order,
			transfer.BookedAt?.InZone(timeZone).ToDateTimeOffset(),
			transfer.ValuedAt?.InZone(timeZone).ToDateTimeOffset());
	}

	internal static TransferSummary ToSummary(
		this Transfer transfer,
		List<Counterparty> counterparties,
		Counterparty userCounterparty,
		(AccountInCurrency AccountInCurrency, Account Account)[] accounts)
	{
		var (sourceCurrency, sourceAccount) = accounts.Single(tuple => tuple.AccountInCurrency.Id == transfer.SourceAccountId);
		var (targetCurrency, targetAccount) = accounts.Single(tuple => tuple.AccountInCurrency.Id == transfer.TargetAccountId);

		return sourceAccount.CounterpartyId == userCounterparty.Id
			? new(
				transfer,
				sourceCurrency.CurrencyAlphabeticCode,
				sourceAccount.PreferredCurrencyId != sourceCurrency.CurrencyId,
				transfer.SourceAmount,
				sourceAccount.Name,
				"→",
				targetAccount.CounterpartyId == userCounterparty.Id,
				counterparties.Single(counterparty => targetAccount.CounterpartyId == counterparty.Id).Name,
				targetAccount.Name,
				targetCurrency.CurrencyAlphabeticCode,
				transfer.TargetAmount)
			: new(
				transfer,
				targetCurrency.CurrencyAlphabeticCode,
				targetAccount.PreferredCurrencyId != targetCurrency.CurrencyId,
				transfer.TargetAmount,
				targetAccount.CounterpartyId == userCounterparty.Id
					? targetAccount.Name
					: counterparties.Single(counterparty => targetAccount.CounterpartyId == counterparty.Id).Name,
				"←",
				false,
				counterparties.Single(counterparty => sourceAccount.CounterpartyId == counterparty.Id).Name,
				sourceAccount.Name,
				sourceCurrency.CurrencyAlphabeticCode,
				transfer.SourceAmount);
	}
}

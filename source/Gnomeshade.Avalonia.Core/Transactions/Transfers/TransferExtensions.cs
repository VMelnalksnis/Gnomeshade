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
			transfer.InternalReference,
			transfer.Order,
			transfer.BookedAt?.InZone(timeZone).ToDateTimeOffset(),
			transfer.ValuedAt?.InZone(timeZone).ToDateTimeOffset());
	}

	internal static TransferSummary ToSummary(
		this Transfer transfer,
		List<Account> accounts,
		IEnumerable<Counterparty> counterparties,
		Counterparty userCounterparty)
	{
		var sourceAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
		var sourceCurrency = sourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId).Currency;
		var targetAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
		var targetCurrency = targetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId).Currency;

		return sourceAccount.CounterpartyId == userCounterparty.Id
			? new(
				sourceCurrency.AlphabeticCode,
				sourceAccount.PreferredCurrency.Id != sourceCurrency.Id,
				transfer.SourceAmount,
				sourceAccount.Name,
				"→",
				targetAccount.CounterpartyId == userCounterparty.Id,
				counterparties.Single(counterparty => targetAccount.CounterpartyId == counterparty.Id).Name,
				targetAccount.Name,
				targetCurrency.AlphabeticCode,
				transfer.TargetAmount)
			: new(
				targetCurrency.AlphabeticCode,
				targetAccount.PreferredCurrency.Id != targetCurrency.Id,
				transfer.TargetAmount,
				targetAccount.Name,
				"←",
				false,
				counterparties.Single(counterparty => sourceAccount.CounterpartyId == counterparty.Id).Name,
				sourceAccount.Name,
				sourceCurrency.AlphabeticCode,
				transfer.SourceAmount);
	}
}

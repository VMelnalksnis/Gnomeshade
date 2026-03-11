// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
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

	internal static TransferOverview ToOverview(
		this PlannedTransfer transfer,
		List<Account> accounts,
		List<Counterparty> counterparties,
		List<Currency> currencies,
		DateTimeZone timeZone)
	{
		string? sourceAccount;
		string? sourceCurrency;
		string? targetAccount;
		string? targetCurrency;

		if (transfer.IsSourceAccount)
		{
			var account = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
			sourceAccount = account.Name;
			sourceCurrency = account.Currencies.Single(c => c.Id == transfer.SourceAccountId).CurrencyAlphabeticCode;
		}
		else
		{
			var counterparty = counterparties.Single(c => c.Id == transfer.SourceCounterpartyId);
			sourceAccount = counterparty.Name;
			sourceCurrency = currencies.Single(currency => currency.Id == transfer.SourceCurrencyId).AlphabeticCode;
		}

		if (transfer.IsTargetAccount)
		{
			var account = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
			targetAccount = account.Name;
			targetCurrency = account.Currencies.Single(c => c.Id == transfer.TargetAccountId).CurrencyAlphabeticCode;
		}
		else
		{
			var counterparty = counterparties.Single(c => c.Id == transfer.TargetCounterpartyId);
			targetAccount = counterparty.Name;
			targetCurrency = currencies.Single(currency => currency.Id == transfer.TargetCurrencyId).AlphabeticCode;
		}

		return new(
			transfer.Id,
			transfer.SourceAmount,
			sourceAccount,
			sourceCurrency,
			transfer.TargetAmount,
			targetAccount,
			targetCurrency,
			transfer.Order,
			transfer.BookedAt?.InZone(timeZone).ToDateTimeOffset(),
			null);
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

	internal static TransferSummary ToSummary(
		this PlannedTransfer plannedTransfer,
		Instant bookedAt,
		IEnumerable<Counterparty> counterparties,
		Counterparty userCounterparty,
		(AccountInCurrency AccountInCurrency, Account Account)[] accounts)
	{
		var (sourceCounterpartyId, sourceCurrencyCode, sourcePreferredCurrencyId, sourceCurrencyId, sourceAccountName) =
			(plannedTransfer.SourceCounterpartyId ?? accounts.Single(tuple => tuple.AccountInCurrency.Id == plannedTransfer.SourceAccountId).Account.CounterpartyId,
			plannedTransfer.SourceAccountId is { } x
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == x).AccountInCurrency.CurrencyAlphabeticCode
				: accounts.First(tuple => tuple.AccountInCurrency.CurrencyId == plannedTransfer.SourceCurrencyId)
					.AccountInCurrency.CurrencyAlphabeticCode,
			plannedTransfer.SourceAccountId is { } y
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == y).Account.PreferredCurrencyId
				: Guid.Empty,
			plannedTransfer.SourceAccountId is { } z
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == z).AccountInCurrency.CurrencyId
				: plannedTransfer.SourceCurrencyId!.Value,
			plannedTransfer.SourceAccountId is { } w
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == w).Account.Name
				: string.Empty);

		var (targetCounterpartyId, targetCurrencyCode, targetPreferredCurrencyId, targetCurrencyId, targetAccountName) =
			(plannedTransfer.TargetCounterpartyId ?? accounts.Single(tuple => tuple.AccountInCurrency.Id == plannedTransfer.TargetAccountId).Account.CounterpartyId,
			plannedTransfer.TargetAccountId is { } a
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == a).AccountInCurrency.CurrencyAlphabeticCode
				: accounts.First(tuple => tuple.AccountInCurrency.CurrencyId == plannedTransfer.TargetCurrencyId)
					.AccountInCurrency.CurrencyAlphabeticCode,
			plannedTransfer.TargetAccountId is { } b
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == b).Account.PreferredCurrencyId
				: Guid.Empty,
			plannedTransfer.TargetAccountId is { } c
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == c).AccountInCurrency.CurrencyId
				: plannedTransfer.TargetCurrencyId!.Value,
			plannedTransfer.TargetAccountId is { } d
				? accounts.Single(tuple => tuple.AccountInCurrency.Id == d).Account.Name
				: string.Empty);

		var transfer = new Transfer
		{
			Id = plannedTransfer.Id,
			CreatedAt = plannedTransfer.CreatedAt,
			OwnerId = plannedTransfer.OwnerId,
			CreatedByUserId = plannedTransfer.CreatedByUserId,
			ModifiedAt = plannedTransfer.ModifiedAt,
			ModifiedByUserId = plannedTransfer.ModifiedByUserId,
			TransactionId = plannedTransfer.TransactionId,
			SourceAmount = plannedTransfer.SourceAmount,
			SourceAccountId = plannedTransfer.SourceAccountId ?? default,
			TargetAmount = plannedTransfer.TargetAmount,
			TargetAccountId = plannedTransfer.TargetAccountId ?? default,
			BankReference = null,
			ExternalReference = null,
			InternalReference = null,
			Order = plannedTransfer.Order,
			BookedAt = bookedAt,
			ValuedAt = null,
		};

		return sourceCounterpartyId == userCounterparty.Id
			? new(
				transfer,
				sourceCurrencyCode,
				sourcePreferredCurrencyId != sourceCurrencyId,
				transfer.SourceAmount,
				sourceAccountName,
				"→",
				targetCounterpartyId == userCounterparty.Id,
				counterparties.Single(counterparty => targetCounterpartyId == counterparty.Id).Name,
				targetAccountName,
				targetCurrencyCode,
				transfer.TargetAmount)
			: new(
				transfer,
				targetCurrencyCode,
				targetPreferredCurrencyId != targetCurrencyId,
				transfer.TargetAmount,
				targetAccountName,
				"←",
				false,
				counterparties.Single(counterparty => sourceCounterpartyId == counterparty.Id).Name,
				sourceAccountName,
				sourceCurrencyCode,
				transfer.SourceAmount);
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Loans.Migration;

/// <summary>Overview of a single <see cref="LegacyLoan"/>.</summary>
public sealed class LegacyLoanRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="LegacyLoanRow"/> class.</summary>
	/// <param name="loan">The loan which this row represents.</param>
	/// <param name="counterparties">A collection of all available counterparties.</param>
	/// <param name="currencies">A collection of all available currencies.</param>
#pragma warning disable CS0612 // Type or member is obsolete
	public LegacyLoanRow(
		LegacyLoan loan,
		IReadOnlyCollection<Counterparty> counterparties,
		IEnumerable<Currency> currencies)
#pragma warning restore CS0612 // Type or member is obsolete
	{
		Issuer = counterparties.Single(counterparty => counterparty.Id == loan.IssuingCounterpartyId).Name;
		Receiver = counterparties.Single(counterparty => counterparty.Id == loan.ReceivingCounterpartyId).Name;
		Amount = loan.Amount;
		Currency = currencies.Single(currency => currency.Id == loan.CurrencyId).AlphabeticCode;
		Id = loan.Id;
	}

	/// <summary>Gets the name of the issuing counterparty of the loan.</summary>
	public string Issuer { get; }

	/// <summary>Gets the name of the issuing counterparty of the loan.</summary>
	public string Receiver { get; }

	/// <summary>Gets the amount loaned or paid back.</summary>
	public decimal Amount { get; }

	/// <summary>Gets the currency of <see cref="Amount"/>.</summary>
	public string Currency { get; }

	internal Guid Id { get; }
}

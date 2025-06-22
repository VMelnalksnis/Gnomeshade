// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;

namespace Gnomeshade.Avalonia.Core.Loans;

/// <summary>Overview of a single <see cref="LoanPayment"/>.</summary>
public sealed class LoanPaymentRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="LoanPaymentRow"/> class.</summary>
	/// <param name="payment">The payment which this row will represent.</param>
	/// <param name="loans">All available loans.</param>
	/// <param name="currencies">All available currencies.</param>
	public LoanPaymentRow(LoanPaymentBase payment, IEnumerable<Loan> loans, IEnumerable<Currency> currencies)
	{
		var loan = loans.Single(loan => loan.Id == payment.LoanId);
		Loan = loan.Name;
		Amount = payment.Amount;
		Interest = payment.Interest;
		Currency = currencies.Single(currency => currency.Id == loan.CurrencyId).AlphabeticCode;
		Id = payment.Id;
	}

	/// <summary>Gets the name of the loan that this payment is a part of.</summary>
	public string Loan { get; }

	/// <summary>Gets the amount that was loaned or paid back.</summary>
	public decimal Amount { get; }

	/// <summary>Gets the interest amount of this loan payment.</summary>
	public decimal Interest { get; }

	/// <summary>Gets the alphabetic code of the currency of <see cref="Amount"/> and <see cref="Interest"/>.</summary>
	public string Currency { get; }

	/// <summary>Gets the point in time when the loan payment was made.</summary>
	public DateTimeOffset? Date { get; init; }

	internal Guid Id { get; }
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;

namespace Gnomeshade.Avalonia.Core.Loans;

/// <summary>Overview of a single <see cref="Loan"/>.</summary>
public sealed class LoanRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="LoanRow"/> class.</summary>
	/// <param name="loan">The loan which this row will represent.</param>
	/// <param name="counterparties">All available counterparties.</param>
	/// <param name="currencies">All available currencies.</param>
	/// <param name="payments">All available loan payments.</param>
	public LoanRow(
		Loan loan,
		IReadOnlyCollection<Counterparty> counterparties,
		IEnumerable<Currency> currencies,
		IEnumerable<LoanPayment> payments)
	{
		Id = loan.Id;
		Name = loan.Name;
		Issuer = counterparties.Single(counterparty => counterparty.Id == loan.IssuingCounterpartyId).Name;
		Receiver = counterparties.Single(counterparty => counterparty.Id == loan.ReceivingCounterpartyId).Name;
		Principal = loan.Principal;
		Currency = currencies.Single(currency => currency.Id == loan.CurrencyId).AlphabeticCode;

		var loanPayments = payments.Where(payment => payment.LoanId == Id).ToArray();
		ActualPrincipal = loanPayments.Where(payment => payment.Amount > 0).Sum(payment => payment.Amount);
		PaidPrincipal = loanPayments.Where(payment => payment.Amount < 0).Sum(payment => -payment.Amount);
		PaidInterest = loanPayments.Where(payment => payment.Amount < 0).Sum(payment => -payment.Interest);
	}

	/// <summary>Gets the name of the loan.</summary>
	public string Name { get; }

	/// <summary>Gets the name of the issuing counterparty.</summary>
	public string Issuer { get; }

	/// <summary>Gets the name of the receiving counterparty.</summary>
	public string Receiver { get; }

	/// <summary>Gets the amount of capital originally borrowed or invested.</summary>
	public decimal Principal { get; }

	/// <summary>Gets the currency of  <see cref="Principal"/>.</summary>
	public string Currency { get; }

	/// <summary>Gets the actual amount borrow as indicated by loan payments.</summary>
	public decimal ActualPrincipal { get; }

	/// <summary>Gets the amount of <see cref="Principal"/> that has been paid back.</summary>
	public decimal PaidPrincipal { get; }

	/// <summary>Gets the amount of interest paid.</summary>
	public decimal PaidInterest { get; }

	/// <summary>Gets the difference between <see cref="Principal"/> and <see cref="PaidPrincipal"/>.</summary>
	public decimal RemainingPrincipal => Principal - PaidPrincipal;

	internal Guid Id { get; }
}

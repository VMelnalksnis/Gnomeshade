// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans;

/// <summary>Overview of a single <see cref="Loan"/>.</summary>
public sealed class LoanOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="LoanOverview"/> class.</summary>
	/// <param name="id">The id of the loan.</param>
	/// <param name="issuingCounterparty">The name of the counterparty that issued the loan.</param>
	/// <param name="receivingCounterparty">The name of the counterparty that received the loan.</param>
	/// <param name="amount">The amount loaned.</param>
	/// <param name="currency">The alphabetic code of the currency of <see cref="Amount"/>.</param>
	public LoanOverview(
		Guid id,
		string issuingCounterparty,
		string receivingCounterparty,
		decimal amount,
		string currency)
	{
		Id = id;
		IssuingCounterparty = issuingCounterparty;
		ReceivingCounterparty = receivingCounterparty;
		Amount = amount;
		Currency = currency;
	}

	/// <summary>Gets the id of the loan.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the counterparty that issued the loan.</summary>
	public string IssuingCounterparty { get; }

	/// <summary>Gets the name of the counterparty that received the loan.</summary>
	public string ReceivingCounterparty { get; }

	/// <summary>Gets the amount loaned.</summary>
	public decimal Amount { get; }

	/// <summary>Gets the alphabetic code of the currency of <see cref="Amount"/>.</summary>
	public string Currency { get; }
}

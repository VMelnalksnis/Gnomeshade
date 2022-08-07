// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a loan.</summary>
/// <seealso cref="Loan"/>
public sealed record LoanCreation : Creation
{
	/// <inheritdoc cref="Loan.IssuingCounterpartyId"/>
	[Required]
	public Guid? IssuingCounterpartyId { get; init; }

	/// <inheritdoc cref="Loan.ReceivingCounterpartyId"/>
	[Required]
	public Guid? ReceivingCounterpartyId { get; init; }

	/// <inheritdoc cref="Loan.Amount"/>
	[Required]
	public decimal? Amount { get; init; }

	/// <inheritdoc cref="Loan.CurrencyId"/>
	[Required]
	public Guid? CurrencyId { get; init; }
}

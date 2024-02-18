// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Loans;

/// <summary>Information needed to create a loan payment.</summary>
/// <seealso cref="Loan"/>
[PublicAPI]
public sealed record LoanPaymentCreation : Creation
{
	/// <inheritdoc cref="LoanPayment.LoanId"/>
	[Required]
	public Guid? LoanId { get; set; }

	/// <inheritdoc cref="LoanPayment.TransactionId"/>
	[Required]
	public Guid? TransactionId { get; set; }

	/// <inheritdoc cref="LoanPayment.Amount"/>
	[Required]
	public decimal? Amount { get; set; }

	/// <inheritdoc cref="LoanPayment.Interest"/>
	[Required]
	public decimal? Interest { get; set; }
}

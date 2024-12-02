// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Loans;

/// <summary>A payment that was to either issue or pay back a loan.</summary>
/// <seealso cref="LoanPaymentCreation"/>
public abstract record LoanPaymentBase
{
	/// <summary>The id of the loan payment.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the loan was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the user that created this loan payment.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The id of the owner of the loan payment.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The point in time when the loan payment was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this loan payment.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The id of the loan that this payment is a part of.</summary>
	/// <seealso cref="Loan"/>
	public Guid LoanId { get; set; }

	/// <summary>The id of the transaction that this payment is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; set; }

	/// <summary>The amount that was loaned or paid back.</summary>
	public decimal Amount { get; set; }

	/// <summary>The interest amount of this loan payment.</summary>
	public decimal Interest { get; set; }
}

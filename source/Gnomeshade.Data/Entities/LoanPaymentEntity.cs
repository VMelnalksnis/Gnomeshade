// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>An amount that was loaned or payed back as a part of a transaction.</summary>
public sealed record LoanPaymentEntity : Entity, IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Gets or sets the id of the loan this loan payment is a part of.</summary>
	/// <seealso cref="Loan2Entity"/>
	public Guid LoanId { get; set; }

	/// <summary>Gets or sets the id of the transaction this loan payment is a part of.</summary>
	/// <seealso cref="TransactionEntity"/>
	public Guid TransactionId { get; set; }

	/// <summary>Gets or sets the amount that was loaned or payed back.</summary>
	public decimal Amount { get; set; }

	/// <summary>Gets or sets the interest amount of this loan payment.</summary>
	public decimal Interest { get; set; }
}

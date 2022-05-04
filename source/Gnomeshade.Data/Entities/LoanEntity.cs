// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>An amount that was loaned or payed back as a part of a transaction.</summary>
public sealed record LoanEntity : IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc />
	public Guid Id { get; init; }

	/// <inheritdoc />
	public Instant CreatedAt { get; init; }

	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Guid CreatedByUserId { get; init; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Gets or sets the id of the the transaction this loan is a part of.</summary>
	/// <seealso cref="TransactionEntity"/>
	public Guid TransactionId { get; set; }

	/// <summary>Gets or sets the id of the counterparty the gave (issued) the loan to <see cref="ReceivingCounterpartyId"/>.</summary>
	/// <seealso cref="CounterpartyEntity"/>
	public Guid IssuingCounterpartyId { get; set; }

	/// <summary>Gets or sets the id of the counterparty the received the loan from <see cref="IssuingCounterpartyId"/>.</summary>
	/// <seealso cref="CounterpartyEntity"/>
	public Guid ReceivingCounterpartyId { get; set; }

	/// <summary>Gets or sets the amount that was loaned or payed back.</summary>
	public decimal Amount { get; set; }

	/// <summary>Gets or sets the id of the currency of the <see cref="Amount"/>.</summary>
	/// <seealso cref="CurrencyEntity"/>
	public Guid CurrencyId { get; set; }
}

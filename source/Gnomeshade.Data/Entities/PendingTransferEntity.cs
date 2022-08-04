// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a pending transfer between two accounts.</summary>
public sealed record PendingTransferEntity : IOwnableEntity, IModifiableEntity
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

	/// <summary>Gets or sets the id of transaction this transfer is a part of.</summary>
	/// <seealso cref="TransactionEntity"/>
	public Guid TransactionId { get; set; }

	/// <summary>Gets or sets the amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; set; }

	/// <summary>Gets or sets the id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	public Guid SourceAccountId { get; set; }

	/// <summary>Gets or sets the id of the counterparty to which currency will be deposited to.</summary>
	/// <seealso cref="CounterpartyEntity"/>
	public Guid TargetCounterpartyId { get; set; }

	/// <summary>Gets or sets the id of the transfer representing the completed transfer.</summary>
	/// <seealso cref="TransferEntity"/>
	public Guid? TransferId { get; set; }
}

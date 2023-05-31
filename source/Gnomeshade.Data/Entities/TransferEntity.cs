// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a transfer between two accounts.</summary>
public sealed record TransferEntity : Entity, IOwnableEntity, IModifiableEntity, ISortableEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

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

	/// <summary>Gets or sets the amount deposited in the target account.</summary>
	public decimal TargetAmount { get; set; }

	/// <summary>Gets or sets the id of the <see cref="AccountInCurrencyEntity"/> to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	public Guid TargetAccountId { get; set; }

	/// <summary>Gets or sets a reference id issued by the bank.</summary>
	public string? BankReference { get; set; }

	/// <summary>Gets or sets a reference id issued by an external source.</summary>
	public string? ExternalReference { get; set; }

	/// <summary>Gets or sets a reference id issued by the user.</summary>
	public string? InternalReference { get; set; }

	/// <inheritdoc />
	public uint? Order { get; set; }

	/// <summary>Gets or sets the point in time when this transfer was posted to an account on the account servicer accounting books.</summary>
	public Instant? BookedAt { get; set; }

	/// <summary>Gets or sets the point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public Instant? ValuedAt { get; set; }
}

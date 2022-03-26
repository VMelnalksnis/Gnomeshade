// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a transfer between two accounts.</summary>
public sealed record TransferEntity : IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc />
	public Guid Id { get; init; }

	/// <inheritdoc />
	public DateTimeOffset CreatedAt { get; init; }

	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Guid CreatedByUserId { get; init; }

	/// <inheritdoc />
	public DateTimeOffset ModifiedAt { get; set; }

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
}

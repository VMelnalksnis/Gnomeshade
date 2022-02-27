// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Products;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>A financial transaction during which funds are transferred between two accounts.</summary>
[PublicAPI]
public sealed record TransactionItem
{
	/// <summary>The id of the transaction item.</summary>
	public Guid Id { get; init; }

	/// <summary>The id of the owner of the transaction item.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the transaction this item is a part of.</summary>
	public Guid TransactionId { get; init; }

	/// <summary>The amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; init; }

	/// <summary>The id of the account from which money was withdrawn.</summary>
	public Guid SourceAccountId { get; init; }

	/// <summary>The amount deposited into the target account.</summary>
	public decimal TargetAmount { get; init; }

	/// <summary>The id of the account into which money was deposited.</summary>
	public Guid TargetAccountId { get; init; }

	/// <summary>The point in time when the transaction item was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the user that created this transaction item.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the transaction item was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this transaction item.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The product that was purchased.</summary>
	public Product Product { get; init; } = null!;

	/// <summary>The amount of product that was purchased.</summary>
	public decimal Amount { get; init; }

	/// <summary>The bank statement reference id/number.</summary>
	public string? BankReference { get; init; }

	/// <summary>An external reference id/number.</summary>
	public string? ExternalReference { get; init; }

	/// <summary>An internal reference id/number.</summary>
	public string? InternalReference { get; init; }

	/// <summary>The point in time at which the product was delivered.</summary>
	public DateTimeOffset? DeliveryDate { get; init; }

	/// <summary>The description of the transaction item.</summary>
	public string? Description { get; init; }
}

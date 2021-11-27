// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities;

/// <summary>
/// Represents the purchase of a single product/service,
/// which can be part of a transaction during which multiple products are purchased.
/// </summary>
public sealed record TransactionItemEntity : IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc/>
	public Guid Id { get; init; }

	/// <inheritdoc/>
	public DateTimeOffset CreatedAt { get; init; }

	/// <inheritdoc/>
	public Guid CreatedByUserId { get; init; }

	/// <inheritdoc/>
	public Guid OwnerId { get; set; }

	/// <inheritdoc/>
	public DateTimeOffset ModifiedAt { get; set; }

	/// <inheritdoc/>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>
	/// Gets or sets the id of the <see cref="TransactionEntity"/> of which this item is a part of.
	/// </summary>
	public Guid TransactionId { get; set; }

	/// <summary>
	/// Gets or sets the amount withdrawn from the source account.
	/// </summary>
	public decimal SourceAmount { get; set; }

	/// <summary>
	/// Gets or sets the id of the <see cref="AccountInCurrencyEntity"/> from which currency is withdrawn from.
	/// </summary>
	public Guid SourceAccountId { get; set; }

	/// <summary>
	/// Gets or sets the amount deposited in the target account.
	/// </summary>
	public decimal TargetAmount { get; set; }

	/// <summary>
	/// Gets or sets the id of the <see cref="AccountInCurrencyEntity"/> to which currency is deposited to.
	/// </summary>
	public Guid TargetAccountId { get; set; }

	/// <summary>
	/// Gets or sets the id of the <see cref="ProductEntity"/> which is exchanged.
	/// </summary>
	public Guid ProductId { get; set; }

	/// <summary>
	/// Gets or sets the product which is exchanged.
	/// </summary>
	public ProductEntity Product { get; set; } = null!;

	/// <summary>
	/// Gets or sets the amount of <see cref="Product"/> exchanged.
	/// </summary>
	public decimal Amount { get; set; }

	/// <summary>
	/// Gets or sets a reference id issued by the bank for this transaction item.
	/// </summary>
	public string? BankReference { get; set; }

	/// <summary>
	/// Gets or sets a reference id issued by an external source.
	/// </summary>
	public string? ExternalReference { get; set; }

	/// <summary>
	/// Gets or sets a reference id issued by the user.
	/// </summary>
	public string? InternalReference { get; set; }

	/// <summary>
	/// Gets or sets the date when the <see cref="Product"/> was delivered.
	/// </summary>
	public DateTimeOffset? DeliveryDate { get; set; }

	/// <summary>
	/// Gets or sets the description of the item.
	/// </summary>
	public string? Description { get; set; }
}

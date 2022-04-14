// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents the purchasing of a product or a service.</summary>
public sealed record PurchaseEntity : IOwnableEntity, IModifiableEntity
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

	/// <summary>Gets or sets the amount paid to purchase an <see cref="Amount"/> of <see cref="ProductId"/>.</summary>
	public decimal Price { get; set; }

	/// <summary>Gets or sets the id of the currency of <see cref="Price"/>.</summary>
	/// <seealso cref="CurrencyEntity"/>
	public Guid CurrencyId { get; set; }

	/// <summary>Gets or sets the id of the purchased product.</summary>
	/// <see cref="ProductEntity"/>
	public Guid ProductId { get; set; }

	/// <summary>Gets or sets the amount of <see cref="ProductId"/> that was purchased.</summary>
	public decimal Amount { get; set; }

	/// <summary>Gets or sets the date when the <see cref="ProductId"/> was delivered.</summary>
	public Instant? DeliveryDate { get; set; }
}

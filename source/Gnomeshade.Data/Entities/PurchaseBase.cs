// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Base model for purchases.</summary>
public abstract record PurchaseBase : Entity, IOwnableEntity, IModifiableEntity, ISortableEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <inheritdoc />
	public uint? Order { get; set; }

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

	/// <summary>Gets or sets the ids of all the projects this purchase is a part of.</summary>
	public List<Guid> ProjectIds { get; set; } = [];
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>The act or an instance of buying of a <see cref="Product"/> as a part of a <see cref="Transaction"/>.</summary>
[PublicAPI]
public sealed record Purchase
{
	/// <summary>The id of the purchase.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the purchase was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the purchase.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this purchase.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the purchase was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this purchase.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The id of transaction this purchase is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; set; }

	/// <summary>The amount paid to purchase an <see cref="Amount"/> of <see cref="ProductId"/>.</summary>
	public decimal Price { get; set; }

	/// <summary>The id of the currency of <see cref="Price"/>.</summary>
	/// <seealso cref="Currency"/>
	public Guid CurrencyId { get; set; }

	/// <summary>The id of the purchased product.</summary>
	/// <see cref="Product"/>
	public Guid ProductId { get; set; }

	/// <summary>The amount of <see cref="ProductId"/> that was purchased.</summary>
	public decimal Amount { get; set; }

	/// <summary>The date when the <see cref="ProductId"/> was delivered.</summary>
	public Instant? DeliveryDate { get; set; }

	/// <summary>The order of the purchase within a transaction.</summary>
	public uint? Order { get; set; }
}

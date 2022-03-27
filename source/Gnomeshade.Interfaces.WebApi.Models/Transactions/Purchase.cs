// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>The act or an instance of buying of a <see cref="Product"/> as a part of a <see cref="Transaction"/>.</summary>
[PublicAPI]
public sealed record Purchase
{
	/// <summary>The id of the purchase.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the purchase was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the owner of the purchase.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this purchase.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the purchase was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this purchase.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The id of transaction this purchase is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; init; }

	/// <summary>The amount paid to purchase an <see cref="Amount"/> of <see cref="ProductId"/>.</summary>
	public decimal Price { get; init; }

	/// <summary>The id of the currency of <see cref="Price"/>.</summary>
	/// <seealso cref="Currency"/>
	public Guid CurrencyId { get; init; }

	/// <summary>The id of the purchased product.</summary>
	/// <see cref="Product"/>
	public Guid ProductId { get; init; }

	/// <summary>The amount of <see cref="ProductId"/> that was purchased.</summary>
	public decimal Amount { get; init; }

	/// <summary>The date when the <see cref="ProductId"/> was delivered.</summary>
	public DateTimeOffset? DeliveryDate { get; init; }
}

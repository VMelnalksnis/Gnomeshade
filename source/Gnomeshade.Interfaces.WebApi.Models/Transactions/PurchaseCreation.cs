// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>Information needed to create a purchase.</summary>
/// <seealso cref="Purchase"/>
[PublicAPI]
public sealed record PurchaseCreation
{
	/// <inheritdoc cref="Purchase.Price"/>
	[Required]
	public decimal? Price { get; set; }

	/// <inheritdoc cref="Purchase.CurrencyId"/>
	[Required]
	public Guid? CurrencyId { get; set; }

	/// <inheritdoc cref="Purchase.ProductId"/>
	[Required]
	public Guid? ProductId { get; set; }

	/// <inheritdoc cref="Purchase.Amount"/>
	[Required]
	public decimal? Amount { get; set; }

	/// <inheritdoc cref="Purchase.DeliveryDate"/>
	public DateTimeOffset? DeliveryDate { get; set; }
}

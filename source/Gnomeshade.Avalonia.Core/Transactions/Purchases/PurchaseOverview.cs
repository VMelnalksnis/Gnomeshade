// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions.Purchases;

/// <summary>Overview of a single <see cref="Purchase"/>.</summary>
public sealed class PurchaseOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="PurchaseOverview"/> class.</summary>
	/// <param name="id">The id of the purchase.</param>
	/// <param name="price">The amount paid to purchase an <see cref="Amount"/> of <see cref="ProductName"/>.</param>
	/// <param name="currencyName">The id of the currency of <see cref="Price"/>.</param>
	/// <param name="productName">The id of the purchased product.</param>
	/// <param name="amount">The amount of <see cref="ProductName"/> that was purchased.</param>
	/// <param name="unitName">The name of the unit in which <see cref="Amount"/> is expressed in.</param>
	/// <param name="deliveryDate">The date when the <see cref="ProductName"/> was delivered.</param>
	public PurchaseOverview(
		Guid id,
		decimal price,
		string currencyName,
		string productName,
		decimal amount,
		string? unitName,
		DateTimeOffset? deliveryDate)
	{
		Id = id;
		Price = price;
		CurrencyName = currencyName;
		ProductName = productName;
		Amount = amount;
		UnitName = unitName;
		DeliveryDate = deliveryDate;
	}

	/// <summary>Gets the id of the purchase.</summary>
	public Guid Id { get; }

	/// <summary>Gets the amount paid to purchase an <see cref="Amount"/> of <see cref="ProductName"/>.</summary>
	public decimal Price { get; }

	/// <summary>Gets the id of the currency of <see cref="Price"/>.</summary>
	public string CurrencyName { get; }

	/// <summary>Gets the id of the purchased product.</summary>
	public string ProductName { get; }

	/// <summary>Gets the amount of <see cref="ProductName"/> that was purchased.</summary>
	public decimal Amount { get; }

	/// <summary>Gets the name of the unit in which <see cref="Amount"/> is expressed in.</summary>
	public string? UnitName { get; }

	/// <summary>Gets the date when the <see cref="ProductName"/> was delivered.</summary>
	public DateTimeOffset? DeliveryDate { get; }
}

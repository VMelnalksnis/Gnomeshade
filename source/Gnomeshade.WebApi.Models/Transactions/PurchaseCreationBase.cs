// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a purchase.</summary>
/// <seealso cref="PurchaseBase"/>
public abstract record PurchaseCreationBase : TransactionItemCreation
{
	/// <inheritdoc cref="PurchaseBase.TransactionId"/>
	[Required]
	public override Guid? TransactionId { get; set; }

	/// <inheritdoc cref="PurchaseBase.Price"/>
	[Required]
	public decimal? Price { get; set; }

	/// <inheritdoc cref="PurchaseBase.CurrencyId"/>
	[Required]
	public Guid? CurrencyId { get; set; }

	/// <inheritdoc cref="PurchaseBase.ProductId"/>
	[Required]
	public Guid? ProductId { get; set; }

	/// <inheritdoc cref="PurchaseBase.Amount"/>
	[Required]
	public decimal? Amount { get; set; }

	/// <inheritdoc cref="PurchaseBase.Order"/>
	public uint? Order { get; set; }
}

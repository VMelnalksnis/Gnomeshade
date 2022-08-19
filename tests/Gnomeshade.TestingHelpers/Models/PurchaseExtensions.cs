// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.TestingHelpers.Models;

public static class PurchaseExtensions
{
	public static PurchaseCreation ToCreation(this Purchase purchase) => new()
	{
		TransactionId = purchase.TransactionId,
		ProductId = purchase.ProductId,
		Amount = purchase.Amount,
		CurrencyId = purchase.CurrencyId,
		Price = purchase.Price,
		DeliveryDate = purchase.DeliveryDate,
	};
}

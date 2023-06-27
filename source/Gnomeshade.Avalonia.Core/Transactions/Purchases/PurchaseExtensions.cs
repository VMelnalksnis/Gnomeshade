// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Transactions.Purchases;

internal static class PurchaseExtensions
{
	internal static PurchaseOverview ToOverview(
		this Purchase purchase,
		IEnumerable<Currency> currencies,
		IEnumerable<Product> products,
		IEnumerable<Unit> units,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		var product = products.Single(product => product.Id == purchase.ProductId);
		var unit = units.SingleOrDefault(unit => unit.Id == product.UnitId);

		return new(
			purchase.Id,
			purchase.Price,
			currencies.Single(currency => currency.Id == purchase.CurrencyId).AlphabeticCode,
			product.Name,
			purchase.Amount,
			unit?.Name,
			purchase.DeliveryDate?.InZone(dateTimeZoneProvider.GetSystemDefault()).LocalDateTime,
			purchase.Order);
	}
}

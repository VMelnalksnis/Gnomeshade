// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;

internal static class PurchaseExtensions
{
	internal static PurchaseOverview ToOverview(
		this Purchase purchase,
		IEnumerable<Currency> currencies,
		IEnumerable<Product> products,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		return new(
			purchase.Id,
			purchase.Price,
			currencies.Single(currency => currency.Id == purchase.CurrencyId).AlphabeticCode,
			products.Single(product => product.Id == purchase.ProductId).Name,
			purchase.Amount,
			purchase.DeliveryDate?.InZone(dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset());
	}
}

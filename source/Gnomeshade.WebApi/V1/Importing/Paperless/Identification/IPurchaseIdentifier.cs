// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.WebApi.V1.Importing.Paperless.Identification;

/// <summary>Identifies purchases from text.</summary>
public interface IPurchaseIdentifier
{
	/// <summary>Identifies the closest matching product, currency and unit from the given values.</summary>
	/// <param name="text">The text from which to identify the purchase.</param>
	/// <param name="products">List of products from which to find a matching one.</param>
	/// <param name="currencies">List of currencies from which to find a matching one.</param>
	/// <param name="units">List of units from which to find a matching one.</param>
	/// <returns>The closest matching purchase to <paramref name="text"/>.</returns>
	IdentifiedPurchase IdentifyPurchase(
		string text,
		IEnumerable<ProductEntity> products,
		IEnumerable<CurrencyEntity> currencies,
		IEnumerable<UnitEntity> units);
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Gnomeshade.WebApi.V1.Importing.Paperless;
using Gnomeshade.WebApi.V1.Importing.Paperless.Rimi;

using Microsoft.Extensions.Logging.Abstractions;

namespace Gnomeshade.WebApi.Tests.V1.Importing.Paperless.Rimi;

[SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Contains test data")]
public sealed class RimiPurchaseIdentifierTestCaseSource : IEnumerable<TestCaseData>
{
	public IEnumerator<TestCaseData> GetEnumerator()
	{
		var identifier = new RimiPurchaseIdentifier(NullLogger<RimiPurchaseIdentifier>.Instance);

		yield return new TestCaseData(
				identifier,
				@"Tostermaize franēu
Brioche 450g
1 gab x 2,55 EUR 2,55 8",
				new IdentifiedPurchase(
					"Tostermaize franēu Brioche 450g",
					PurchaseIdentifierTests.Products.Single(),
					88,
					"EUR",
					2.55m,
					450,
					"g"))
			.SetName("Existing product");

		yield return new TestCaseData(
				identifier,
				@"siefs Dor blu 50% kg
Or 188 kg X 14,59 EUR/kg 2,748",
				new IdentifiedPurchase(
					"siefs Dor blu 50% kg",
					PurchaseIdentifierTests.Products.Single(),
					30,
					"EUR",
					2.74m,
					188,
					null))
			.SetName("New product");

		yield return new TestCaseData(
				identifier,
				@"Sviests Exporta 82,5% 200g
1 gab X 3,09 EUR 3,09 A
Atl -0,50 Gala cena 2,59",
				new IdentifiedPurchase(
					"Sviests Exporta 82,5% 200g",
					PurchaseIdentifierTests.Products.Single(),
					23,
					"EUR",
					2.59m,
					200,
					"g"))
			.SetName("New product with discount");

		yield return new TestCaseData(
				identifier,
				@"Sviests Exporta 82,5% 200g
1 gab X 3,09 EUR 3,09 A
ati -0,50 Gala cena 2,59",
				new IdentifiedPurchase(
					"Sviests Exporta 82,5% 200g",
					PurchaseIdentifierTests.Products.Single(),
					23,
					"EUR",
					2.59m,
					200,
					"g"))
			.SetName("New product with discount, bad OCR");
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

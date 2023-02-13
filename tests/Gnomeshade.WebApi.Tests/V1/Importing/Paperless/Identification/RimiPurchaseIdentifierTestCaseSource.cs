// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Gnomeshade.WebApi.V1.Importing.Paperless.Identification;

using Microsoft.Extensions.Logging.Abstractions;

namespace Gnomeshade.WebApi.Tests.V1.Importing.Paperless.Identification;

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
					"Tostermaize franču Brioche",
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
					"Tostermaize franču Brioche",
					30,
					"EUR",
					2.74m,
					188,
					null))
			.SetName("New product");
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

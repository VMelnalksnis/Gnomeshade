// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Gnomeshade.WebApi.V1.Importing.Paperless.Parsing;

using VMelnalksnis.PaperlessDotNet.Documents;

namespace Gnomeshade.WebApi.Tests.V1.Importing.Paperless.Parsing;

public sealed class PaperlessDocumentParserTests
{
	[TestCaseSource(typeof(RimiReceiptParserTestCaseSource))]
	public void ParsePurchases_ShouldReturnExpected(
		IPaperlessDocumentParser parser,
		Document document,
		List<string> expectedPurchases)
	{
		var purchases = parser.ParsePurchases(document);

		purchases.Should().BeEquivalentTo(expectedPurchases);
	}
}

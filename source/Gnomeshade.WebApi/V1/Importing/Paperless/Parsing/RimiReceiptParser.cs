// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using VMelnalksnis.PaperlessDotNet.Documents;

namespace Gnomeshade.WebApi.V1.Importing.Paperless.Parsing;

/// <inheritdoc />
public sealed class RimiReceiptParser : IPaperlessDocumentParser
{
	private const StringComparison _comparison = StringComparison.OrdinalIgnoreCase;
	private const string _startIdentifier = "KLIENT";

	private static readonly (string, string)[] _replace =
	{
		("_", " "),
		("é", "ē"),
		("°", string.Empty),
		("’", string.Empty),
		("'", string.Empty),
		("\"", string.Empty),
	};

	private static readonly string[] _endIdentifiers = { "Citas akcijas", "ATLALDES", "Makeajanu karte" };

	private readonly ILogger<RimiReceiptParser> _logger;

	/// <summary>Initializes a new instance of the <see cref="RimiReceiptParser"/> class.</summary>
	/// <param name="logger">Logger in the specific category.</param>
	public RimiReceiptParser(ILogger<RimiReceiptParser> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public List<string> ParsePurchases(Document document)
	{
		// Replace parsing artifacts
		var content = _replace
			.Aggregate(document.Content, (current, tuple) => current.Replace(tuple.Item1, tuple.Item2));

		// Find start and end by expected text before and after list of purchases
		var start = content.IndexOf(_startIdentifier, _comparison) + _startIdentifier.Length;
		var end = _endIdentifiers
			.Select(filter => content.LastIndexOf(filter, _comparison))
			.FirstOrDefault(index => index is not -1);

		var productPart = content[start..end];

		// Remove extra whitespace
		var newStart = productPart.IndexOf("\n\n", _comparison);
		if (newStart is not -1)
		{
			start = newStart;
		}

		var newEnd = productPart.LastIndexOf("\n\n", _comparison);
		if (newEnd is not -1)
		{
			end = newEnd;
			productPart = productPart[start..end];
		}

		_logger.LogTrace("Extracted products from document content {ProductPart}", productPart);

		// Filter lines that don't contain any text, only OCR artifacts
		var lines = productPart
			.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
			.Where(line => line.Any(c => char.IsDigit(c) || char.IsLetter(c)))
			.ToList();

		var rawProducts = new List<string>();

		while (lines.Count is not 0)
		{
			// Find next line with currency
			var index = lines.FindIndex(line => line.Contains("EUR", _comparison));
			if (index is -1)
			{
				throw new NotSupportedException(string.Join(Environment.NewLine, lines));
			}

			// Check if next line contains discounted price
			if (index != lines.Count - 1 &&
				(lines[index + 1].Contains("Atl. ", _comparison) || lines[index + 1].Contains("Atl ", _comparison)))
			{
				index++;
			}

			rawProducts.Add(string.Join(Environment.NewLine, lines.Take(index + 1)));
			lines = lines.Skip(index + 1).ToList();
		}

		_logger.LogTrace("Extracted products {RawProducts} from document content", rawProducts);

		return rawProducts;
	}
}

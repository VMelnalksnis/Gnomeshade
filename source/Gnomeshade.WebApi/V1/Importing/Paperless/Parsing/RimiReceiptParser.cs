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
	private const string _startIdentifier = "\n\n\n\n\n";

	private static readonly (string, string)[] _replace =
	{
		("_", " "),
		("é", "ē"),
		("°", string.Empty),
		("’", " "),
		("'", " "),
		("\"", string.Empty),
		("“", string.Empty),
		("|", string.Empty),
	};

	// todo need a better way to catch all parsing errors
	private static readonly string[] _endIdentifiers =
	{
		"ATLALDES",
		"Citas akcijas",
		"Makeajanu karte",
		"Makeajamu karte",
	};

	private static readonly string[] _discountIdentifiers =
	{
		"Atl. ",
		"Atl ",
		"ati ",
	};

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

		if (start > end)
		{
			start = content.IndexOf(_startIdentifier[..^1], _comparison) + _startIdentifier.Length;
		}

		var productPart = content[start..end].Trim('\n');
		var startIndex = productPart.IndexOf("KLIENT", StringComparison.Ordinal);
		if (startIndex >= 0)
		{
			var newStart = productPart.IndexOf("\n", startIndex + 6, StringComparison.Ordinal);
			productPart = productPart[newStart..].TrimStart('\n');
		}

		_logger.LogDebug("Extracted products from document content {ProductPart}", productPart);

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
				_discountIdentifiers.Any(identifier => lines[index + 1].StartsWith(identifier, _comparison)))
			{
				index++;
			}

			rawProducts.Add(string.Join(Environment.NewLine, lines.Take(index + 1)));
			lines = lines.Skip(index + 1).ToList();
		}

		_logger.LogDebug("Extracted products {RawProducts} from document content", rawProducts);

		return rawProducts;
	}
}

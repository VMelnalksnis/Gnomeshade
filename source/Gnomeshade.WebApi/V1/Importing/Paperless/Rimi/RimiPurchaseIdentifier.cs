﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using FuzzySharp;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

using static Gnomeshade.WebApi.V1.Importing.Paperless.Rimi.Constants;

namespace Gnomeshade.WebApi.V1.Importing.Paperless.Rimi;

/// <inheritdoc />
public sealed partial class RimiPurchaseIdentifier : IPurchaseIdentifier
{
	private readonly ILogger<RimiPurchaseIdentifier> _logger;

	/// <summary>Initializes a new instance of the <see cref="RimiPurchaseIdentifier"/> class.</summary>
	/// <param name="logger">Logger for the specific category.</param>
	public RimiPurchaseIdentifier(ILogger<RimiPurchaseIdentifier> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public IdentifiedPurchase IdentifyPurchase(
		string text,
		IEnumerable<ProductEntity> products,
		IEnumerable<CurrencyEntity> currencies,
		IEnumerable<UnitEntity> units)
	{
		var lines = text.Split(Environment.NewLine);
		var purchase = GetPurchase(lines);

		var currencyCodes = currencies.Select(currency => currency.AlphabeticCode);
		var currency = GetPurchaseCurrency(purchase, currencyCodes);

		var price = GetPurchasePrice(purchase);

		var unitSymbols = units.Where(unit => unit.Symbol is not null).Select(unit => unit.Symbol!).ToList();
		var amount = GetProductAmount(purchase, unitSymbols);

		var match = products
			.Select(product => (product, Fuzz.Ratio(product.Name, purchase.Product)))
			.DefaultIfEmpty((new(), 0))
			.MaxBy(tuple => tuple.Item2);

		var identifiedPurchase = new IdentifiedPurchase(
			purchase.Product,
			match.product,
			match.Item2,
			currency,
			price,
			amount.Amount,
			amount.Unit);

		_logger.LogTrace("Identified {IdentifiedPurchase} from {RawPurchaseText}", identifiedPurchase, text);
		return identifiedPurchase;
	}

	[GeneratedRegex(@"\d{1,},\d{2}", RegexOptions.IgnoreCase)]
	private static partial Regex PriceRegex();

	private static RimiPurchase GetPurchase(string[] lines) => lines.Length switch
	{
		< 2 => throw new ArgumentException("Not enough lines to parse product", nameof(lines)),

		2 => new(lines[0], lines[1], null),

		_ when DiscountIdentifiers.All(identifier => !lines[^1].StartsWith(identifier, Comparison)) =>
			new(string.Join(" ", lines[..^1]), lines[^1], null),

		_ => new(string.Join(" ", lines[..^2]), lines[^2], lines[^1]),
	};

	private static decimal GetPurchasePrice(RimiPurchase purchase)
	{
		var amountText = purchase.DiscountAndFinalPrice is null
			? PriceRegex().Matches(purchase.AmountAndPrice).Last().Value
			: PriceRegex().Matches(purchase.DiscountAndFinalPrice).Last().Value;

		return decimal.Parse(amountText, NumberStyles.Float, CultureInfo.GetCultureInfo("lv-LV"));
	}

	private static (decimal Amount, string? Unit) GetProductAmount(
		RimiPurchase purchase,
		IReadOnlyCollection<string> unitAbbr)
	{
		var amountText = Regex.Matches(purchase.AmountAndPrice, @"[\d,]{1,}").First().Value;
		var amount = decimal.Parse(amountText, NumberStyles.Float, CultureInfo.GetCultureInfo("lv-LV"));

		var unit = unitAbbr
			.FirstOrDefault(abbr =>
				purchase.Product.EndsWith(abbr, Comparison) &&
				char.IsDigit(purchase.Product[..^abbr.Length].Last()));

		if (unit is null)
		{
			var withoutAmount = purchase.AmountAndPrice[amountText.Length..].TrimStart().Split(' ').FirstOrDefault();
			unit = unitAbbr.FirstOrDefault(abbr => StringComparer.OrdinalIgnoreCase.Equals(abbr, withoutAmount));

			return (amount, unit);
		}

		var unitAmountText = Regex.Matches(purchase.Product, @"\d{1,}").Last().Value;
		var unitAmount = decimal.Parse(unitAmountText, NumberStyles.Float, CultureInfo.GetCultureInfo("lv-LV"));

		return (amount * unitAmount, unit);
	}

	private string GetPurchaseCurrency(RimiPurchase purchase, IEnumerable<string> currencyCodes)
	{
		var currency = currencyCodes
			.FirstOrDefault(code => purchase.AmountAndPrice.Contains($" {code}", Comparison));

		if (currency is null)
		{
			throw new InvalidOperationException($"Could not find currency for purchase {purchase}");
		}

		return currency;
	}

	private sealed record RimiPurchase(string Product, string AmountAndPrice, string? DiscountAndFinalPrice);
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.WebApi.V1.Importing.Paperless;

/// <summary>A purchase that has been identified from text.</summary>
/// <param name="OriginalName">The exact product name from the parsed text.</param>
/// <param name="ClosestProductName">The closest matching product name to <see cref="OriginalName"/>.</param>
/// <param name="Score">A score from 0 to 100 indicating how close <see cref="ClosestProductName"/> is to <see cref="OriginalName"/>.</param>
/// <param name="Currency">The alphabetic code of the currency of <see cref="Price"/>.</param>
/// <param name="Price">The price of the purchase.</param>
/// <param name="Amount">The amount of product purchased.</param>
/// <param name="Unit">The name of the unit of <see cref="Amount"/>.</param>
public sealed record IdentifiedPurchase(
	string OriginalName,
	string ClosestProductName,
	int Score,
	string Currency,
	decimal Price,
	decimal Amount,
	string? Unit);

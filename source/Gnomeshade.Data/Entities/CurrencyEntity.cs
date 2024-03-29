﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents an ISO 4217 currency.</summary>
public sealed record CurrencyEntity : Entity, INamedEntity
{
	/// <inheritdoc />
	public string Name { get; set; } = null!;

	/// <inheritdoc />
	public string NormalizedName { get; set; } = null!;

	/// <summary>Gets the ISO 4217 three-digit numeric code.</summary>
	public short NumericCode { get; init; }

	/// <summary>Gets the ISO 4217 three-character alphabetic code.</summary>
	public string AlphabeticCode { get; init; } = null!;

	/// <summary>
	/// Gets the base 10 ratio of the minor unit to the currency.
	/// <c>0</c> means there is no minor unit and <c>2</c> means 100 minor units equal 1 unit of the currency,
	/// for example cents in USD and EUR.
	/// </summary>
	public byte MinorUnit { get; init; }

	/// <summary>Gets a value indicating whether or not the currency is officially recognized in ISO 4217.</summary>
	public bool Official { get; init; }

	/// <summary>Gets a value indicating whether or not the currency is a cryptocurrency.</summary>
	public bool Crypto { get; init; }

	/// <summary>Gets a value indicating whether or not the currency is no longer active.</summary>
	public bool Historical { get; init; }

	/// <summary>Gets the date from which the currency has been active.</summary>
	public Instant? ActiveFrom { get; init; }

	/// <summary>Gets the date until which the currency has been/will be active.</summary>
	public Instant? ActiveUntil { get; init; }
}

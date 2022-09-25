// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

#pragma warning disable SA1623
namespace Gnomeshade.WebApi.Models.Accounts;

/// <summary>
/// A currency used in transactions.
/// </summary>
/// <seealso href="https://en.wikipedia.org/wiki/ISO_4217"/>
[PublicAPI]
public sealed record Currency
{
	/// <summary>
	/// The id of the currency.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// The point in time when this currency was created.
	/// </summary>
	public Instant CreatedAt { get; set; }

	/// <summary>
	/// The name of the currency.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// The ISO 4217 three digit numeric code.
	/// </summary>
	public short NumericCode { get; set; }

	/// <summary>
	/// The ISO 4217 three letter alphabetic code.
	/// </summary>
	public string AlphabeticCode { get; set; } = null!;

	/// <summary>
	/// The number of minor unit decimal places.
	/// </summary>
	public byte MinorUnit { get; set; }

	/// <summary>
	/// A value indicating whether this currency is listed in ISO 4217.
	/// </summary>
	public bool Official { get; set; }

	/// <summary>
	/// A value indicating whether this currency is a cryptocurrency.
	/// </summary>
	public bool Crypto { get; set; }

	/// <summary>
	/// A value indicating whether this currency is no longer being used.
	/// </summary>
	public bool Historical { get; set; }

	/// <summary>
	/// The point of time from which this currency has been used.
	/// </summary>
	public Instant? ActiveFrom { get; set; }

	/// <summary>
	/// The point of time until which this currency has been used.
	/// </summary>
	public Instant? ActiveUntil { get; set; }
}

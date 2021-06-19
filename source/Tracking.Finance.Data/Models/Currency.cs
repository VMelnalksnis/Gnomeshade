// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	/// <summary>
	/// Represents an ISO 4217 currency.
	/// </summary>
	public sealed record Currency : INamedEntity
	{
		/// <inheritdoc />
		public Guid Id { get; init; }

		/// <inheritdoc />
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc />
		public string Name { get; set; } = string.Empty;

		/// <inheritdoc />
		public string NormalizedName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the ISO 4217 three-digit numeric code.
		/// </summary>
		public short NumericCode { get; set; }

		/// <summary>
		/// Gets or sets the ISO 4217 three-character alphabetic code.
		/// </summary>
		public string AlphabeticCode { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the base 10 ratio of the minor unit to the currency.
		/// <c>0</c> means there is no minor unit and <c>2</c> means 100 minor units equal 1 unit of the currency,
		/// for example cents in USD and EUR.
		/// </summary>
		public byte MinorUnit { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not the currency is officially recognized in ISO 4217.
		/// </summary>
		public bool Official { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not the currency is a cryptocurrency.
		/// </summary>
		public bool Crypto { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not the currency is no longer active.
		/// </summary>
		public bool Historical { get; set; }

		/// <summary>
		/// Gets or sets the date from which the currency has been active.
		/// </summary>
		public DateTimeOffset? ActiveFrom { get; set; }

		/// <summary>
		/// Gets or sets the date until which the currency has been/will be active.
		/// </summary>
		public DateTimeOffset? ActiveUntil { get; set; }
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Accounts
{
	[PublicAPI]
	public sealed record CurrencyModel
	{
		/// <summary>
		/// The id of the currency.
		/// </summary>
		public Guid Id { get; init; }

		/// <summary>
		/// The point in time when this currency was created.
		/// </summary>
		public DateTimeOffset CreatedAt { get; init; }

		/// <summary>
		/// The name of the currency.
		/// </summary>
		public string Name { get; init; } = null!;

		/// <summary>
		/// The ISO 4217 three digit numeric code.
		/// </summary>
		public short NumericCode { get; init; }

		/// <summary>
		/// The ISO 4217 three letter alphabetic code.
		/// </summary>
		public string AlphabeticCode { get; init; } = null!;

		/// <summary>
		/// The number of minor unit decimal places.
		/// </summary>
		public byte MinorUnit { get; init; }

		/// <summary>
		/// A value indicating whether this currency is listed in ISO 4217.
		/// </summary>
		public bool Official { get; init; }

		/// <summary>
		/// A value indicating whether this currency is a cryptocurrency.
		/// </summary>
		public bool Crypto { get; init; }

		/// <summary>
		/// A value indicating whether this currency is no longer being used.
		/// </summary>
		public bool Historical { get; init; }

		/// <summary>
		/// The point of time from which this currency has been used.
		/// </summary>
		public DateTimeOffset? ActiveFrom { get; init; }

		/// <summary>
		/// The point of time until which this currency has been used.
		/// </summary>
		public DateTimeOffset? ActiveUntil { get; init; }
	}
}

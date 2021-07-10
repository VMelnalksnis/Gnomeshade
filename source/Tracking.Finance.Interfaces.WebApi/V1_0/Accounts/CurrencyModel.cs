// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	public sealed record CurrencyModel
	{
		public Guid Id { get; init; }

		public DateTimeOffset CreatedAt { get; init; }

		public string Name { get; init; }

		public short NumericCode { get; init; }

		public string AlphabeticCode { get; init; }

		public byte MinorUnit { get; init; }

		public bool Official { get; init; }

		public bool Crypto { get; init; }

		public bool Historical { get; init; }

		public DateTimeOffset? ActiveFrom { get; init; }

		public DateTimeOffset? ActiveUntil { get; init; }
	}
}

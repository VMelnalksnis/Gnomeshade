// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class Currency : IEntity, INamedEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public short NumericCode { get; set; }

		public string AlphabeticCode { get; set; }

		public string? Symbol { get; set; }

		public short MinorUnit { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		public bool Official { get; set; }

		public bool Crypto { get; set; }

		public bool Historical { get; set; }

		public DateTimeOffset? From { get; set; }

		public DateTimeOffset? Until { get; set; }
	}
}

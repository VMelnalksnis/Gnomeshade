// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class Account : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity, IDescribableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int UserId { get; set; }

		public int? CounterpartyId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public int CreatedByUserId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public int ModifiedByUserId { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		/// <inheritdoc/>
		public string? Description { get; set; }

		public string? Bic { get; set; }

		public string? Iban { get; set; }

		public string? AccountNumber { get; set; }

		public bool Active { get; set; }

		public int PrefferedCurrencyId { get; set; }

		public bool LimitCurrencies { get; set; }
	}
}

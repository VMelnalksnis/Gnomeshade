// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	/// <summary>
	/// Represents an account which can hold funds one or more currencies.
	/// </summary>
	public sealed record Account : IOwnableEntity, IModifiableEntity, INamedEntity
	{
		/// <inheritdoc />
		public Guid Id { get; init; }

		/// <inheritdoc />
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc />
		public Guid OwnerId { get; set; }

		/// <inheritdoc />
		public Guid CreatedByUserId { get; init; }

		/// <inheritdoc />
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc />
		public Guid ModifiedByUserId { get; set; }

		/// <inheritdoc />
		public string Name { get; set; } = string.Empty;

		/// <inheritdoc />
		public string NormalizedName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the id of the preferred <see cref="AccountInCurrency"/>.
		/// </summary>
		public Guid PreferredCurrencyId { get; set; }

		/// <summary>
		/// Gets or sets the Business Identifier Code (BIC).
		/// </summary>
		public string? Bic { get; set; }

		/// <summary>
		/// Gets or sets the International Bank Account Number (IBAN).
		/// </summary>
		public string? Iban { get; set; }

		/// <summary>
		/// Gets or sets the account number, which does not follow standards such as IBAN.
		/// </summary>
		public string? AccountNumber { get; set; }
	}
}

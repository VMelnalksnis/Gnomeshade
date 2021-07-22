// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;
using Tracking.Finance.Data.Repositories.Extensions;

namespace Tracking.Finance.Data.Models
{
	/// <summary>
	/// Represents a single currency of an <see cref="Account"/>.
	/// </summary>
	public sealed record AccountInCurrency : IOwnableEntity, IModifiableEntity
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

		/// <summary>
		/// Gets or sets the id of the <see cref="Account"/> which holds this currency.
		/// </summary>
		public Guid AccountId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Currency"/> this account represents.
		/// </summary>
		public Guid CurrencyId { get; set; }

		/// <summary>
		/// Gets or sets the currency this account represents.
		/// </summary>
		public Currency Currency { get; set; } = null!;

		internal static AccountInCurrency Create(OneToOne<AccountInCurrency, Currency> relationship)
		{
			relationship.First.Currency = relationship.Second;
			return relationship.First;
		}
	}
}

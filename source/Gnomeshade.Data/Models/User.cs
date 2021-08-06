// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models.Abstractions;

namespace Gnomeshade.Data.Models
{
	public sealed record User : IModifiableEntity
	{
		/// <inheritdoc/>
		public Guid Id { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public Guid ModifiedByUserId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Counterparty"/> which represents this user in transactions.
		/// </summary>
		public Guid CounterpartyId { get; set; }
	}
}

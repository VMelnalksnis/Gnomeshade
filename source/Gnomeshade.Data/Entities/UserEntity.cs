// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities
{
	/// <summary>
	/// A user within the context of this application.
	/// </summary>
	public sealed record UserEntity : IModifiableEntity
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
		/// Gets or sets the id of the <see cref="CounterpartyEntity"/> which represents this user in transactions.
		/// </summary>
		public Guid CounterpartyId { get; set; }
	}
}

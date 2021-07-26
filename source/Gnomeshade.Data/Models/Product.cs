// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models.Abstractions;

namespace Gnomeshade.Data.Models
{
	/// <summary>
	/// Represents a product that is exchanged during a transaction.
	/// </summary>
	public sealed record Product : IOwnableEntity, IModifiableEntity, INamedEntity
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
		public string Name { get; set; } = null!;

		/// <inheritdoc />
		public string NormalizedName { get; set; } = null!;

		/// <summary>
		/// Gets or sets the description of this product.
		/// </summary>
		public string? Description { get; set; }

		public Guid? UnitId { get; set; }
	}
}

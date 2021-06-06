// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class Product : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity, IDescribableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int UserId { get; set; }

		public int CategoryId { get; set; }

		public int UnitId { get; set; }

		public int SupplierId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public int CreatedByUserId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public int ModifiedByUserId { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; } = null!;

		/// <inheritdoc/>
		public string NormalizedName { get; set; } = null!;

		/// <inheritdoc/>
		public string? Description { get; set; }
	}
}

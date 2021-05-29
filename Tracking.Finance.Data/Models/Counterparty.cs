using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class Counterparty : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int UserId { get; set; }

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
	}
}

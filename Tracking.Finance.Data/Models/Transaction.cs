using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class Transaction : IEntity, IUserSpecificEntity, IModifiableEntity
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

		public DateTimeOffset Date { get; set; }

		public string? Description { get; set; }

		public bool Generated { get; set; }

		public bool Validated { get; set; }

		public bool Completed { get; set; }
	}
}

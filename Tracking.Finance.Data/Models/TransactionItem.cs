using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class TransactionItem : IEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int UserId { get; set; }

		public int TransactionId { get; set; }

		public decimal SourceAmount { get; set; }

		public int SourceAccountId { get; set; }

		public decimal TargetAmount { get; set; }

		public int TargetAccountId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public int CreatedByUserId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public int ModifiedByUserId { get; set; }

		public int ProductId { get; set; }

		public decimal Amount { get; set; }

		public string? BankReference { get; set; }

		public string? ExternalReference { get; set; }

		public string? InternalReference { get; set; }

		public DateTimeOffset? DeliveryDate { get; set; }

		public string? Description { get; set; }
	}
}

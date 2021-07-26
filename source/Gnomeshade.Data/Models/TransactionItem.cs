// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models.Abstractions;

namespace Gnomeshade.Data.Models
{
	public sealed record TransactionItem : IOwnableEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public Guid Id { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc/>
		public Guid CreatedByUserId { get; init; }

		/// <inheritdoc/>
		public Guid OwnerId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public Guid ModifiedByUserId { get; set; }

		public Guid TransactionId { get; set; }

		public decimal SourceAmount { get; set; }

		public Guid SourceAccountId { get; set; }

		public decimal TargetAmount { get; set; }

		public Guid TargetAccountId { get; set; }

		public Guid ProductId { get; set; }

		public Product Product { get; set; } = null!;

		public decimal Amount { get; set; }

		public string? BankReference { get; set; }

		public string? ExternalReference { get; set; }

		public string? InternalReference { get; set; }

		public DateTimeOffset? DeliveryDate { get; set; }

		public string? Description { get; set; }
	}
}

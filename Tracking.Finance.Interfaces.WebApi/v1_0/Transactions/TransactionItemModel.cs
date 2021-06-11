// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	public sealed record TransactionItemModel
	{
		public Guid Id { get; init; }

		public Guid UserId { get; init; }

		public Guid TransactionId { get; init; }

		public decimal SourceAmount { get; init; }

		public Guid SourceAccountId { get; init; }

		public decimal TargetAmount { get; init; }

		public Guid TargetAccountId { get; init; }

		public DateTimeOffset CreatedAt { get; init; }

		public Guid CreatedByUserId { get; init; }

		public DateTimeOffset ModifiedAt { get; init; }

		public Guid ModifiedByUserId { get; init; }

		public Guid ProductId { get; init; }

		public decimal Amount { get; init; }

		public string? BankReference { get; init; }

		public string? ExternalReference { get; init; }

		public string? InternalReference { get; init; }

		public DateTimeOffset? DeliveryDate { get; init; }

		public string? Description { get; init; }
	}
}

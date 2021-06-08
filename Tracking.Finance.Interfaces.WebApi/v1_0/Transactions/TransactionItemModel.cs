// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	public record TransactionItemModel(
		Guid Id,
		Guid UserId,
		Guid TransactionId,
		decimal SourceAmount,
		Guid SourceAccountId,
		decimal TargetAmount,
		Guid TargetAccountId,
		DateTimeOffset CreatedAt,
		Guid CreatedByUserId,
		DateTimeOffset ModifiedAt,
		Guid ModifiedByUserId,
		Guid ProductId,
		decimal Amount,
		string? BankReference,
		string? ExternalReference,
		string? GuidernalReference,
		DateTimeOffset? DeliveryDate,
		string? Description)
	{
		public Guid Id { get; init; } = Id;

		public Guid UserId { get; init; } = UserId;

		public Guid TransactionId { get; init; } = TransactionId;

		public decimal SourceAmount { get; init; } = SourceAmount;

		public Guid SourceAccountId { get; init; } = SourceAccountId;

		public decimal TargetAmount { get; init; } = TargetAmount;

		public Guid TargetAccountId { get; init; } = TargetAccountId;

		public DateTimeOffset CreatedAt { get; init; } = CreatedAt;

		public Guid CreatedByUserId { get; init; } = CreatedByUserId;

		public DateTimeOffset ModifiedAt { get; init; } = ModifiedAt;

		public Guid ModifiedByUserId { get; init; } = ModifiedByUserId;

		public Guid ProductId { get; init; } = ProductId;

		public decimal Amount { get; init; } = Amount;

		public string? BankReference { get; init; } = BankReference;

		public string? ExternalReference { get; init; } = ExternalReference;

		public string? GuidernalReference { get; init; } = GuidernalReference;

		public DateTimeOffset? DeliveryDate { get; init; } = DeliveryDate;

		public string? Description { get; init; } = Description;
	}
}

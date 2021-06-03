using System;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	public record TransactionItemModel(
		int Id,
		int UserId,
		int TransactionId,
		decimal SourceAmount,
		int SourceAccountId,
		decimal TargetAmount,
		int TargetAccountId,
		DateTimeOffset CreatedAt,
		int CreatedByUserId,
		DateTimeOffset ModifiedAt,
		int ModifiedByUserId,
		int ProductId,
		decimal Amount,
		string? BankReference,
		string? ExternalReference,
		string? InternalReference,
		DateTimeOffset? DeliveryDate,
		string? Description)
	{
		public int Id { get; init; } = Id;

		public int UserId { get; init; } = UserId;

		public int TransactionId { get; init; } = TransactionId;

		public decimal SourceAmount { get; init; } = SourceAmount;

		public int SourceAccountId { get; init; } = SourceAccountId;

		public decimal TargetAmount { get; init; } = TargetAmount;

		public int TargetAccountId { get; init; } = TargetAccountId;

		public DateTimeOffset CreatedAt { get; init; } = CreatedAt;

		public int CreatedByUserId { get; init; } = CreatedByUserId;

		public DateTimeOffset ModifiedAt { get; init; } = ModifiedAt;

		public int ModifiedByUserId { get; init; } = ModifiedByUserId;

		public int ProductId { get; init; } = ProductId;

		public decimal Amount { get; init; } = Amount;

		public string? BankReference { get; init; } = BankReference;

		public string? ExternalReference { get; init; } = ExternalReference;

		public string? InternalReference { get; init; } = InternalReference;

		public DateTimeOffset? DeliveryDate { get; init; } = DeliveryDate;

		public string? Description { get; init; } = Description;
	}
}

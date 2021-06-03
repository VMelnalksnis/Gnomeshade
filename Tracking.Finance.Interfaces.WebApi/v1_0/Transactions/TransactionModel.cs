using System;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	public record TransactionModel(
		int Id,
		int UserId,
		DateTimeOffset CreatedAt,
		int CreatedByUserId,
		DateTimeOffset ModifiedAt,
		DateTimeOffset Date,
		string? Description,
		bool Generated,
		bool Validated,
		bool Completed)
	{
		public int Id { get; init; } = Id;

		public int UserId { get; init; } = UserId;

		public DateTimeOffset CreatedAt { get; init; } = CreatedAt;

		public int CreatedByUserId { get; init; } = CreatedByUserId;

		public DateTimeOffset ModifiedAt { get; init; } = ModifiedAt;

		public DateTimeOffset Date { get; init; } = Date;

		public string? Description { get; init; } = Description;

		public bool Generated { get; init; } = Generated;

		public bool Validated { get; init; } = Validated;

		public bool Completed { get; init; } = Completed;
	}
}

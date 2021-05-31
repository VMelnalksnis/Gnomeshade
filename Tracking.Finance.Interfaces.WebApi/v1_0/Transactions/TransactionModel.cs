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
		public int Id { get; set; } = Id;

		public int UserId { get; set; } = UserId;

		public DateTimeOffset CreatedAt { get; set; } = CreatedAt;

		public int CreatedByUserId { get; set; } = CreatedByUserId;

		public DateTimeOffset ModifiedAt { get; set; } = ModifiedAt;

		public DateTimeOffset Date { get; set; } = Date;

		public string? Description { get; set; } = Description;

		public bool Generated { get; set; } = Generated;

		public bool Validated { get; set; } = Validated;

		public bool Completed { get; set; } = Completed;
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
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

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	public sealed record TransactionModel
	{
		public Guid Id { get; init; }

		public Guid UserId { get; init; }

		public DateTimeOffset CreatedAt { get; init; }

		public Guid CreatedByUserId { get; init; }

		public DateTimeOffset ModifiedAt { get; init; }

		public DateTimeOffset Date { get; init; }

		public string? Description { get; init; }

		public bool Generated { get; init; }

		public bool Validated { get; init; }

		public bool Completed { get; init; }

		public List<TransactionItemModel> Items { get; init; }
	}
}

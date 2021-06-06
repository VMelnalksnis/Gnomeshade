// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	public record TransactionItemCreationModel
	{
		[Required]
		public decimal? SourceAmount { get; init; }

		[Required]
		public int? SourceAccountId { get; init; }

		[Required]
		public decimal? TargetAmount { get; init; }

		[Required]
		public int? TargetAccountId { get; init; }

		[Required]
		public int? ProductId { get; init; }

		[Required]
		public decimal? Amount { get; init; }

		public string? BankReference { get; init; }

		public string? ExternalReference { get; init; }

		public string? InternalReference { get; init; }

		public DateTimeOffset? DeliveryDate { get; init; }

		public string? Description { get; init; }
	}
}

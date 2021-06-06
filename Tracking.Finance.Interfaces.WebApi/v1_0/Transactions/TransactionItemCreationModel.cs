// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	public class TransactionItemCreationModel
	{
		[Required]
		public int? UserId { get; set; }

		[Required]
		public decimal? SourceAmount { get; set; }

		[Required]
		public int? SourceAccountId { get; set; }

		[Required]
		public decimal? TargetAmount { get; set; }

		[Required]
		public int? TargetAccountId { get; set; }

		[Required]
		public int? ProductId { get; set; }

		[Required]
		public decimal? Amount { get; set; }

		public string? BankReference { get; set; }

		public string? ExternalReference { get; set; }

		public string? InternalReference { get; set; }

		public DateTimeOffset? DeliveryDate { get; set; }

		public string? Description { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Transactions
{
	public class TransactionCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		public int? TransactionCategoryId { get; set; }

		[Required]
		public int? SourceAccountId { get; set; }

		[Required]
		public int? TargetAccountId { get; set; }

		[DataType(DataType.Date)]
		public DateTimeOffset? CompletedAt { get; set; }

		[DataType(DataType.Time)]
		public DateTime? CompletedAtTime { get; set; }

		[Required]
		public int? CounterpartyId { get; set; }

		[DataType(DataType.Text)]
		public string? BankReference { get; set; }

		[DataType(DataType.Text)]
		public string? ExternalReference { get; set; }

		[DataType(DataType.Text)]
		public string? InternalReference { get; set; }

		[DataType(DataType.MultilineText)]
		public string? Description { get; set; }

		public bool Completed { get; set; }

		public IEnumerable<SelectListItem> Categories { get; set; }

		public IEnumerable<SelectListItem> Accounts { get; set; }

		public IEnumerable<SelectListItem> Counterparties { get; set; }
	}
}

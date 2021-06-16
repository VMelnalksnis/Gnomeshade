using System;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Web.Models.Transactions
{
	public class TransactionCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[DataType(DataType.Date)]
		public DateTimeOffset? Date { get; set; }

		[DataType(DataType.Time)]
		public DateTime? Time { get; set; }

		[DataType(DataType.MultilineText)]
		public string? Description { get; set; }

		public bool Completed { get; set; }
	}
}

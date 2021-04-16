using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models
{
	public class TransactionItemCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		public int? TransactionId { get; set; }

		public decimal SourceAmount { get; set; }

		[Required]
		public int? SourceCurrencyId { get; set; }

		public decimal TargetAmount { get; set; }

		[Required]
		public int? TargetCurrencyId { get; set; }

		[Required]
		public int? ProductId { get; set; }

		public decimal Amount { get; set; }

		[DataType(DataType.MultilineText)]
		public string? Description { get; set; }

		public IEnumerable<SelectListItem> Currencies { get; set; }

		public IEnumerable<SelectListItem> Products { get; set; }
	}
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		public string Name { get; set; }

		public bool SingleCurrency { get; set; }

		public int? CurrencyId { get; set; }

		public IEnumerable<SelectListItem> Currencies { get; set; }
	}
}

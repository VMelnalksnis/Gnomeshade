using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AddCurrencyModel
	{
		public int? AccountId { get; set; }

		public int? CurrencyId { get; set; }

		public List<SelectListItem> Currencies { get; set; }
	}
}

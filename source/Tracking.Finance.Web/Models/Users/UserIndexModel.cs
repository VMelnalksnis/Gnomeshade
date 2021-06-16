using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Users
{
	public class UserIndexModel
	{
		public string IdentityUserId { get; set; }

		public string IdentityUserName { get; set; }

		public int? CounterpartyId { get; set; }

		public string? CounterpartyName { get; set; }

		public List<SelectListItem> Counterparties { get; set; }
	}
}

using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Rendering;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	/// <summary>
	/// Information needed in order to add an <see cref="AccountInCurrency"/> to an <see cref="Account"/>.
	/// </summary>
	public class AddCurrencyModel
	{
		/// <summary>
		/// Gets the id of the <see cref="Account"/> to which to add the <see cref="Currency"/> to.
		/// </summary>
		public int? AccountId { get; init; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Currency"/> which to add to the <see cref="Account"/>.
		/// </summary>
		public int? CurrencyId { get; set; }

		/// <summary>
		/// Gets a collection of <see cref="SelectListItem"/> containing available currencies that can be added to the <see cref="Account"/>.
		/// </summary>
		public List<SelectListItem> Currencies { get; init; }
	}
}

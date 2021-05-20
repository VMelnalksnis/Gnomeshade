using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	/// <summary>
	/// Information needed in order to create an <see cref="Account"/>.
	/// </summary>
	public class AccountCreationModel
	{
		/// <summary>
		/// Gets the id of the <see cref="FinanceUser"/> to which the account belongs to.
		/// </summary>
		[Required]
		public int? FinanceUserId { get; init; }

		/// <summary>
		/// Gets or sets the name of the <see cref="Account"/>.
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string? Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="Account"/> supports only a single <see cref="Currency"/>.
		/// </summary>
		public bool SingleCurrency { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="Account"/> is owned by the user.
		/// </summary>
		public bool UserAccount { get; set; }

		/// <summary>
		/// Gets or sets the id of the first <see cref="Currency"/> that will be added to the <see cref="Account"/>.
		/// </summary>
		public int? CurrencyId { get; set; }

		/// <summary>
		/// Gets a collection of <see cref="SelectListItem"/> containing available currencies that can be selected for <see cref="CurrencyId"/>.
		/// </summary>
		public List<SelectListItem> Currencies { get; init; } = new List<SelectListItem>();
	}
}

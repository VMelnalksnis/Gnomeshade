using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Rendering;

using Tracking.Finance.Web.Data.Models;

using VMelnalksnis.SvgCharts.Charts;

namespace Tracking.Finance.Web.Models.Accounts
{
	/// <summary>
	/// Information about a specific <see cref="Account"/>.
	/// </summary>
	public record AccountDetailsViewModel(int Id, string Name, bool SingleCurrency, LineChart Chart, List<SelectListItem> CurrencyListItems, List<AccountDetailsCurrencyViewModel> Currencies)
	{
		/// <summary>
		/// Gets the id of the <see cref="Account"/>.
		/// </summary>
		public int Id { get; init; } = Id;

		/// <summary>
		/// Gets the name of the <see cref="Account"/>.
		/// </summary>
		public string Name { get; init; } = Name;

		/// <summary>
		/// Gets a value indicating whether the account can contain only a single currency.
		/// </summary>
		public bool SingleCurrency { get; init; } = SingleCurrency;

		public LineChart Chart { get; init; } = Chart;

		/// <summary>
		/// Gets a collection of <see cref="SelectListItem"/> containing available currencies that can be added to the <see cref="Account"/>.
		/// </summary>
		public List<SelectListItem> CurrencyListItems { get; init; } = CurrencyListItems;

		/// <summary>
		/// Gets a collection of <see cref="AccountDetailsCurrencyViewModel"/> containing details about <see cref="Account"/> balance in different currencies.
		/// </summary>
		public List<AccountDetailsCurrencyViewModel> Currencies { get; init; } = Currencies;
	}
}

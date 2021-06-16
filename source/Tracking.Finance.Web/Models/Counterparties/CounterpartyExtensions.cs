using System.Collections.Generic;
using System.Linq;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Counterparties
{
	public static class CounterpartyExtensions
	{
		public static IEnumerable<CounterpartyIndexModel> GetViewModels(this IEnumerable<Counterparty> counterparties)
		{
			return counterparties.Select(counterparty => new CounterpartyIndexModel(counterparty.Id, counterparty.Name));
		}
	}
}

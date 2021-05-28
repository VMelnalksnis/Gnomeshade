using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Tracking.Finance.Web.Data.Models
{
	public class FinanceUser : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		public int? CounterpartyId { get; set; }

		public IdentityUser IdentityUser { get; set; }

		[ForeignKey(nameof(CounterpartyId))]
		public Counterparty? Counterparty { get; set; }
	}
}

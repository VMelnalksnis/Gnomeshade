using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class User : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		public int? CounterpartyId { get; set; }
	}
}

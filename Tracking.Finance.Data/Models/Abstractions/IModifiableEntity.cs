using System;

namespace Tracking.Finance.Data.Models.Abstractions
{
	public interface IModifiableEntity
	{
		DateTimeOffset CreatedAt { get; set; }

		public int CreatedByUserId { get; set; }

		DateTimeOffset ModifiedAt { get; set; }

		public int ModifiedByUserId { get; set; }
	}
}

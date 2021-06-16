using System;

namespace Tracking.Finance.Web.Data.Models
{
	public interface IModifiableEntity
	{
		DateTimeOffset CreatedAt { get; set; }

		DateTimeOffset ModifiedAt { get; set; }
	}
}

using System;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Models
{
	public class AuthenticatedUser
	{
		public string Token { get; set; }

		public DateTimeOffset ValidTo { get; set; }
	}
}

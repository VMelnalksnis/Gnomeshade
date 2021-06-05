using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.Tests.Integration
{
	public record AuthenticatedUserOptions
	{
		[Required(AllowEmptyStrings = false)]
		public string Username { get; init; }

		[Required(AllowEmptyStrings = false)]
		public string Password { get; init; }
	}
}

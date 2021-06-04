using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public class RegistrationModel
	{
		[Required(AllowEmptyStrings = false)]
		public string Username { get; set; }

		[EmailAddress]
		[Required(AllowEmptyStrings = false)]
		public string Email { get; set; }

		[Required(AllowEmptyStrings = false)]
		public string Password { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public class LoginModel
	{
		[Required(AllowEmptyStrings = false)]
		public string Username { get; set; }

		[Required(AllowEmptyStrings = false)]
		public string Password { get; set; }
	}
}

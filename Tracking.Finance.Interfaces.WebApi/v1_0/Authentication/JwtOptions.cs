using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public class JwtOptions
	{
		/// <summary>
		/// The name of the configuration section.
		/// </summary>
		public const string SectionName = "JWT";

		[Required(AllowEmptyStrings = false)]
		public string ValidAudience { get; set; }

		[Required(AllowEmptyStrings = false)]
		public string ValidIssuer { get; set; }

		[Required(AllowEmptyStrings = false)]
		public string Secret { get; set; }

		public SymmetricSecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
	}
}

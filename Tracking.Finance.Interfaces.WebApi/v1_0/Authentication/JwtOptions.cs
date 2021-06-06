// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Authentication
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

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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

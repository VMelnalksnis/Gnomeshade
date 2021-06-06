// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Authentication
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

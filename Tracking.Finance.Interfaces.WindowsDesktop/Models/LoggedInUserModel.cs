// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Interfaces.WindowsDesktop.Models
{
	public record LoggedInUserModel
	{
		public string Id { get; set; }

		public string UserName { get; set; }

		public string Email { get; set; }
	}
}

// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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

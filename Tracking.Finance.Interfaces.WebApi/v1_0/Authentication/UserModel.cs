// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public record UserModel
	{
		public string Id { get; init; }

		public string UserName { get; init; }

		public string Email { get; init; }
	}
}

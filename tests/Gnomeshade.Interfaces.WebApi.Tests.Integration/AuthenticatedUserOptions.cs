// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration
{
	public record AuthenticatedUserOptions
	{
		[Required(AllowEmptyStrings = false)]
		public string Username { get; init; }

		[Required(AllowEmptyStrings = false)]
		public string Password { get; init; }
	}
}

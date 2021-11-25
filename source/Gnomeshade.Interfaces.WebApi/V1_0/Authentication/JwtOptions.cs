// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Authentication
{
	public sealed record JwtOptions
	{
		[Required]
		public string ValidAudience { get; init; } = null!;

		[Required]
		public string ValidIssuer { get; init; } = null!;

		[Required]
		public string Secret { get; init; } = null!;

		public SymmetricSecurityKey SecurityKey => new(Encoding.UTF8.GetBytes(Secret));
	}
}

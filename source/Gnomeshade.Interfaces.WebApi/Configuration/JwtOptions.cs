// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

/// <summary>Options for built-in user authentication.</summary>
public sealed record JwtOptions
{
	/// <inheritdoc cref="JwtBearerOptions.Audience"/>
	/// <seealso cref="JwtBearerOptions.Audience"/>
	/// <seealso cref="TokenValidationParameters.ValidAudience"/>
	[Required]
	public string ValidAudience { get; init; } = null!;

	/// <inheritdoc cref="AuthenticationSchemeOptions.ClaimsIssuer"/>
	/// <seealso cref="AuthenticationSchemeOptions.ClaimsIssuer"/>
	/// <seealso cref="TokenValidationParameters.ValidIssuer"/>
	[Required]
	public string ValidIssuer { get; init; } = null!;

	/// <summary>Gets the string value of <see cref="SecurityKey"/>.</summary>
	[Required]
	public string Secret { get; init; } = null!;

	/// <inheritdoc cref="TokenValidationParameters.IssuerSigningKey"/>
	/// <seealso cref="TokenValidationParameters.IssuerSigningKey"/>
	public SymmetricSecurityKey SecurityKey => new(Encoding.UTF8.GetBytes(Secret));
}

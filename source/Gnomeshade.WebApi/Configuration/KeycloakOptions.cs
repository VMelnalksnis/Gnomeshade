// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Gnomeshade.WebApi.Configuration;

/// <summary>Options for configuration Keycloak OIDC provider.</summary>
public sealed class KeycloakOptions
{
	/// <inheritdoc cref="JwtBearerOptions.Authority"/>
	/// <seealso cref="JwtBearerOptions.Authority"/>
	[Required]
	public Uri ServerRealm { get; init; } = null!;

	/// <inheritdoc cref="JwtBearerOptions.MetadataAddress"/>
	/// <seealso cref="JwtBearerOptions.MetadataAddress"/>
	[Required]
	public Uri Metadata { get; init; } = null!;

	/// <inheritdoc cref="JwtBearerOptions.Audience"/>
	/// <seealso cref="JwtBearerOptions.Audience"/>
	[Required]
	public string ClientId { get; init; } = null!;
}

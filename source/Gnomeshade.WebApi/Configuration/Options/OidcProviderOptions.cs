// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Gnomeshade.WebApi.Configuration.Options;

/// <summary>Options for configuring an OIDC provider.</summary>
public sealed class OidcProviderOptions
{
	/// <summary>The name of the section under which to place provider sections.</summary>
	internal const string OidcProviderSectionName = "Oidc";

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
	/// <seealso cref="OpenIdConnectOptions.ClientId"/>
	[Required]
	public string ClientId { get; init; } = null!;

	/// <inheritdoc cref="OpenIdConnectOptions.ClientSecret"/>
	/// <seealso cref="OpenIdConnectOptions.ClientSecret"/>
	public string? ClientSecret { get; init; }

	/// <inheritdoc cref="JwtBearerOptions.RequireHttpsMetadata"/>
	/// <seealso cref="JwtBearerOptions.RequireHttpsMetadata"/>
	public bool RequireHttpsMetadata { get; init; } = true;

	/// <summary>Gets a display name for the authentication handler. Defaults to the section name if not specified.</summary>
	public string? DisplayName { get; init; }
}

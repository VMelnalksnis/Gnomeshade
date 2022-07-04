// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using IdentityModel.OidcClient;

namespace Gnomeshade.Interfaces.Avalonia.Core.Configuration;

/// <summary>Settings needed to configure an OIDC client.</summary>
public sealed class OidcOptions
{
	/// <inheritdoc cref="OidcClientOptions.Authority"/>
	[Required]
	public Uri Authority { get; set; } = null!;

	/// <inheritdoc cref="OidcClientOptions.ClientId"/>
	[Required]
	public string ClientId { get; set; } = null!;

	/// <inheritdoc cref="OidcClientOptions.ClientSecret"/>
	public string? ClientSecret { get; set; }

	/// <inheritdoc cref="OidcClientOptions.RedirectUri"/>
	public Uri RedirectUri => new("http://localhost:8297");

	/// <inheritdoc cref="OidcClientOptions.Scope"/>
	public string Scope => "openid profile";
}

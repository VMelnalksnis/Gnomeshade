﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using IdentityModel.OidcClient;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Settings needed to configure an OIDC client.</summary>
public sealed class OidcOptions
{
	/// <inheritdoc cref="OidcClientOptions.Authority"/>
	[Required]
	public Uri? Authority { get; set; }

	/// <inheritdoc cref="OidcClientOptions.ClientId"/>
	[Required]
	public string ClientId { get; set; } = "gnomeshade-desktop";

	/// <inheritdoc cref="OidcClientOptions.ClientSecret"/>
	public string? ClientSecret { get; set; }

	/// <inheritdoc cref="OidcClientOptions.RedirectUri"/>
	[JsonIgnore]
	public Uri RedirectUri => new("gnomeshade://localhost");

	/// <inheritdoc cref="OidcClientOptions.Scope"/>
	[JsonIgnore]
	public string Scope => "openid profile offline_access";

	/// <summary>Gets or sets the time in seconds to wait until OIDC signin is completed by the user.</summary>
	public int? SigninTimeoutSeconds { get; set; }

	/// <summary>Gets the time to wait until OIDC signin is completed by the user.</summary>
	[JsonIgnore]
	public TimeSpan SigninTimeout => TimeSpan.FromSeconds(SigninTimeoutSeconds ?? 60);
}

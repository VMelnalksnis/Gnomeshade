// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authentication.OAuth;

namespace Gnomeshade.WebApi.Configuration.Options;

/// <summary>Options for configuring an OAuth provider.</summary>
public sealed class OAuthProviderOptions
{
	internal const string ProviderSectionName = "OAuth";

	/// <inheritdoc cref="OAuthOptions.ClientId"/>
	[Required]
	public string ClientId { get; init; } = null!;

	/// <inheritdoc cref="OAuthOptions.ClientSecret"/>
	public string? ClientSecret { get; init; }
}

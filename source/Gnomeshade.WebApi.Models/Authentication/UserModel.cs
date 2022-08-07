// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Authentication;

/// <summary>Information about a user.</summary>
[PublicAPI]
public sealed record UserModel
{
	/// <summary>The id of the user.</summary>
	public string Id { get; init; } = null!;

	/// <summary>The username of the user.</summary>
	public string Username { get; init; } = null!;

	/// <summary>The primary email address of the user.</summary>
	public string Email { get; init; } = null!;
}

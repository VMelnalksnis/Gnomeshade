// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Authentication;

/// <summary>The information needed to register a new user.</summary>
[PublicAPI]
public sealed record RegistrationModel
{
	/// <inheritdoc cref="UserModel.Username"/>
	[Required]
	public string Username { get; set; } = null!;

	/// <inheritdoc cref="UserModel.Email"/>
	[EmailAddress]
	[Required]
	public string Email { get; set; } = null!;

	/// <summary>The password of the user.</summary>
	[Required]
	public string Password { get; set; } = null!;

	/// <summary>The full name of the user.</summary>
	[Required]
	public string FullName { get; set; } = null!;
}

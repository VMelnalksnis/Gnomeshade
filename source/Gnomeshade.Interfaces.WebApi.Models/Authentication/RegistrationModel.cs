// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Authentication;

/// <summary>
/// The information needed to register a new user.
/// </summary>
[PublicAPI]
public sealed record RegistrationModel
{
	[Required(AllowEmptyStrings = false)]
	public string Username { get; init; } = null!;

	[EmailAddress]
	[Required(AllowEmptyStrings = false)]
	public string Email { get; init; } = null!;

	[Required(AllowEmptyStrings = false)]
	public string Password { get; init; } = null!;

	[Required(AllowEmptyStrings = false)]
	public string FullName { get; init; } = null!;
}

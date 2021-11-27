// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Entities;

using Microsoft.AspNetCore.Authorization;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

/// <summary>
/// Specifies that the class or method that this attribute is applied to requires access to
/// the current application user <see cref="UserEntity"/>.
/// </summary>
/// <seealso cref="ApplicationUserContext"/>
/// <seealso cref="ApplicationUserHandler"/>
public sealed class AuthorizeApplicationUserAttribute : AuthorizeAttribute
{
	/// <summary>
	/// The name of the application user authorization policy.
	/// </summary>
	public const string PolicyName = nameof(AuthorizeApplicationUserAttribute);

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthorizeApplicationUserAttribute"/> class.
	/// </summary>
	public AuthorizeApplicationUserAttribute()
		: base(PolicyName)
	{
	}
}

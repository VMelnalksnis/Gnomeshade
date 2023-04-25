// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Entities;
using Gnomeshade.WebApi.Configuration;

using Microsoft.AspNetCore.Authorization;

namespace Gnomeshade.WebApi.V1.Authorization;

/// <summary>
/// Specifies that the class or method that this attribute is applied to requires access to
/// the current application user <see cref="UserEntity"/>.
/// </summary>
/// <seealso cref="ApplicationUserContext"/>
/// <seealso cref="ApplicationUserHandler"/>
public sealed class AuthorizeApplicationUserAttribute : AuthorizeAttribute
{
	/// <summary>Initializes a new instance of the <see cref="AuthorizeApplicationUserAttribute"/> class.</summary>
	public AuthorizeApplicationUserAttribute()
		: base(Policies.ApplicationUser)
	{
	}
}

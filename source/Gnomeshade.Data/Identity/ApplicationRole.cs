// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.AspNetCore.Identity;

namespace Gnomeshade.Data.Identity;

/// <summary>Application identity role.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
	/// <summary>Initializes a new instance of the <see cref="ApplicationRole"/> class.</summary>
	public ApplicationRole()
	{
		Id = Guid.NewGuid();
	}

	/// <summary>Initializes a new instance of the <see cref="ApplicationRole"/> class.</summary>
	/// <param name="roleName">The role name.</param>
	public ApplicationRole(string roleName)
		: this()
	{
		Name = roleName;
	}
}

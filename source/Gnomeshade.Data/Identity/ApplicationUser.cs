// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Identity;

namespace Gnomeshade.Data.Identity;

/// <summary>Application identity user.</summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed class ApplicationUser : IdentityUser<Guid>
{
	/// <summary>Initializes a new instance of the <see cref="ApplicationUser"/> class.</summary>
	public ApplicationUser()
	{
		Id = Guid.NewGuid();
		SecurityStamp = Guid.NewGuid().ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="ApplicationUser"/> class.</summary>
	/// <param name="userName">The username.</param>
	public ApplicationUser(string userName)
		: this()
	{
		UserName = userName;
	}

	/// <summary>Gets or sets the full name for this user.</summary>
	// ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
	public string FullName { get; set; } = null!;
}

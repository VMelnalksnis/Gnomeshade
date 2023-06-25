// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;

namespace Gnomeshade.WebApi.V1.Authorization;

/// <summary>Authorization handler which handles the <see cref="ApplicationUserRequirement"/>.</summary>
public sealed class ApplicationUserHandler : AuthorizationHandler<ApplicationUserRequirement>
{
	private readonly UserRepository _userRepository;

	/// <summary>Initializes a new instance of the <see cref="ApplicationUserHandler"/> class.</summary>
	/// <param name="userRepository">The repository for performing CRUD operations on <see cref="UserEntity"/>.</param>
	public ApplicationUserHandler(UserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	/// <inheritdoc />
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ApplicationUserRequirement requirement)
	{
		if (!context.User.TryGetUserId(out var id))
		{
			context.Fail(new(this, "User does not have valid id claim"));
			return;
		}

		var user = await _userRepository.FindById(id);
		if (user is not null)
		{
			context.Succeed(requirement);
		}
		else
		{
			context.Fail(new(this, "User is not an application user"));
		}
	}
}

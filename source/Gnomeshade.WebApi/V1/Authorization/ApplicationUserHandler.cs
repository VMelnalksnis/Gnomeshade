// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;

namespace Gnomeshade.WebApi.V1.Authorization;

/// <summary>Authorization handler which handles the <see cref="ApplicationUserRequirement"/>.</summary>
public sealed class ApplicationUserHandler : AuthorizationHandler<ApplicationUserRequirement>
{
	/// <inheritdoc />
	protected override Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ApplicationUserRequirement requirement)
	{
		if (context.User.TryGetUserId(out _))
		{
			context.Succeed(requirement);
			return Task.CompletedTask;
		}

		context.Fail(new(this, "User does not have valid id claim"));
		return Task.CompletedTask;
	}
}
